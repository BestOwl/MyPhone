using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Device.Services;
using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using GoodTimeStudio.MyPhone.OBEX.Pbap;
using Microsoft.Extensions.Logging;
using MixERP.Net.VCards;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.Device
{
    public class DevicePhonebookServiceProvider : BaseDeviceServiceProvider
    {
        public PbapClient? PbapClient => _pbapClientSession?.ObexClient;

        private readonly BluetoothDevice _device;
        private readonly IContactStore _contactStore;
        private readonly IDeviceConfiguration _deviceConfiguration;
        private readonly ILogger<DevicePhonebookServiceProvider> _logger;

        private BluetoothPbapClientSession? _pbapClientSession;

        public DevicePhonebookServiceProvider(
            BluetoothDevice bluetoothDevice, 
            IContactStore contactStore,
            IDeviceConfiguration deviceConfiguration,
            ILogger<DevicePhonebookServiceProvider> logger) : base(bluetoothDevice)
        {
            _device = bluetoothDevice;
            _contactStore = contactStore;
            _deviceConfiguration = deviceConfiguration;
            _logger = logger;
        }

        protected async override Task<bool> ConnectToServiceAsync()
        {
            try
            {
                _logger.LogInformation(AppLogEvents.PhonebookServiceConnect, "Connecting to PhonebookService.");
                _pbapClientSession = new BluetoothPbapClientSession(_device);
                _logger.LogInformation(AppLogEvents.PhonebookServiceConnect, "Starting PbapClient");
                await _pbapClientSession.ConnectAsync();
                _logger.LogInformation(AppLogEvents.PhonebookServiceConnect, "PbapClient connected to server.");

                if (!_deviceConfiguration.PhonebookServiceSyncedTime.HasValue)
                {
                    _ = SyncPhonebookAsync();
                }

                return true;
            }
            catch (BluetoothDeviceNotAvailableException)
            {
                _pbapClientSession!.Dispose();
                _pbapClientSession = null;
                _logger.LogWarning(AppLogEvents.SmsServiceConnect, "Failed to connect SmsService. The remote Bluetooth device is not available.");
                return false;
            }
            catch (BluetoothObexSessionException ex)
            {
                _pbapClientSession!.Dispose();
                _pbapClientSession = null;
                _logger.LogWarning(AppLogEvents.SmsServiceConnect, ex, "Failed to connect SmsService. ");
                throw new DeviceServiceException(ex.Message, ex);
            }
        }

        public async Task SyncPhonebookAsync()
        {
            _logger.LogInformation("Synchronizing Phonebook...");
            if (_pbapClientSession == null)
            {
                throw new InvalidOperationException("PhonebookService has not been initialized.");
            }
            Debug.Assert(_pbapClientSession.ObexClient != null);
            var contactVCards = await _pbapClientSession.ObexClient.GetAllContactsAsync();
            await _contactStore.ClearStoreAsync();

            List<Contact> contacts = new List<Contact>();
            foreach (VCard card in contactVCards)
            {
                contacts.Add(new Contact
                {
                    MiddleName = card.MiddleName ?? string.Empty,
                    NickName = card.NickName ?? string.Empty,
                    FirstName = card.FirstName ?? string.Empty,
                    LastName = card.LastName ?? string.Empty,
                    FormattedName = card.FormattedName ?? string.Empty,
                    Organization = card.Organization ?? string.Empty,
                    OrganizationalUnit = card.OrganizationalUnit ?? string.Empty,
                    Detail = card
                });
            }
            await _contactStore.AddRangeAsync(contacts);
            DateTime now = DateTime.Now;
            _deviceConfiguration.PhonebookServiceSyncedTime = now;
            _logger.LogInformation("Synced {ContactsCount} contacts at {Time}", contacts.Count, now);
        }

        protected override void OnDisconnected()
        {
            _logger.LogWarning(AppLogEvents.PhonebookServiceDisconnect, "PbapClient disconnected.");
            base.OnDisconnected();
        }

        public override void Dispose()
        {
            _pbapClientSession?.Dispose();

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

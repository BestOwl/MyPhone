using GoodTimeStudio.MyPhone.Device.Services;
using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using GoodTimeStudio.MyPhone.OBEX.Pbap;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DevicePhonebookServiceProvider> _logger;

        private BluetoothPbapClientSession? _pbapClientSession;

        public DevicePhonebookServiceProvider(
            BluetoothDevice bluetoothDevice, 
            IContactStore contactStore,
            ILogger<DevicePhonebookServiceProvider> logger) : base(bluetoothDevice)
        {
            _device = bluetoothDevice;
            _contactStore = contactStore;
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
            if (_pbapClientSession == null)
            {
                throw new InvalidOperationException("PhonebookService has not been initialized.");
            }
            Debug.Assert(_pbapClientSession.ObexClient != null);
            var contacts = (await _pbapClientSession.ObexClient.GetAllContactsAsync()).ToList();

            // TODO: implement vCard UUID
            throw new NotImplementedException();
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

using CommunityToolkit.Mvvm.DependencyInjection;
using GoodTimeStudio.MyPhone.OBEX;
using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using GoodTimeStudio.MyPhone.OBEX.Map;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// Bluetooth Message Access Profile Manager
    /// </summary>
    public class DeviceSmsServiceProvider : BaseDeviceServiceProvider
    {
        public MasClient? MasClient => _masClientSession?.ObexClient;

        private readonly BluetoothDevice _device;
        private readonly ILogger<DeviceSmsServiceProvider> _logger;
        private readonly IMessageNotificationService _notificationService;

        private BluetoothMasClientSession? _masClientSession;
        private BluetoothMnsServerSession _mnsServerSession;

        private bool _firstStart = true;

        public DeviceSmsServiceProvider(BluetoothDevice bluetoothDevice, 
            ILogger<DeviceSmsServiceProvider> logger, IMessageNotificationService messageNotificationService) : base(bluetoothDevice)
        {
            _device = bluetoothDevice;
            _logger = logger;
            _notificationService = messageNotificationService;
            _mnsServerSession = new BluetoothMnsServerSession();
        }

        protected override async Task<bool> ConnectToServiceAsync()
        {
            try
            {
                _logger.LogInformation(AppLogEvents.SmsServiceConnect, "Connecting to SmsService.");
                _masClientSession = new BluetoothMasClientSession(_device);
                _logger.LogInformation(AppLogEvents.SmsServiceConnect, "Starting MasClient");
                await _masClientSession.ConnectAsync();
                _logger.LogInformation(AppLogEvents.SmsServiceConnect, "MasClient connected to server.");
                if (_firstStart)
                {
                    _logger.LogInformation("Starting MnsServer.");
                    await _mnsServerSession.StartServerAsync();
                    _mnsServerSession.ClientAccepted += _mnsServerSession_ClientAccepted;
                    _logger.LogInformation("MnsServer started.");
                    _firstStart = false;
                }
                else
                {
                    _logger.LogInformation(AppLogEvents.SmsServiceConnect, "MnsServer already started, skip.");
                }

                Debug.Assert(_masClientSession.ObexClient != null);
                _logger.LogInformation(AppLogEvents.SmsServiceConnect, "Register message notification.");
                await _masClientSession.ObexClient.SetNotificationRegistrationAsync(true);
                _logger.LogInformation(AppLogEvents.SmsServiceConnect, "SmsService connected.");
                return true;
            }
            catch (BluetoothDeviceNotAvailableException) 
            {
                _masClientSession!.Dispose();
                _masClientSession = null;
                _logger.LogWarning(AppLogEvents.SmsServiceConnect, "Failed to connect SmsService. The remote Bluetooth device is not available.");
                return false;
            }
            catch (BluetoothObexSessionException ex)
            {
                _masClientSession!.Dispose();
                _masClientSession = null;
                _logger.LogWarning(AppLogEvents.SmsServiceConnect, ex, "Failed to connect SmsService. ");
                throw new DeviceServiceException(ex.Message, ex);
            }
        }

        protected override void OnDisconnected()
        {
            _logger.LogWarning(AppLogEvents.SmsServiceDisconnect, "SmsService disconnected.");
            base.OnDisconnected();
        }

        private void _mnsServerSession_ClientAccepted(BluetoothObexServerSession<MnsServer> sender, BluetoothObexServerSessionClientAcceptedEventArgs<MnsServer> e)
        {
            _logger.LogInformation(AppLogEvents.SmsServiceMnsAccepted, "MnsServer accepted a notification client.");
            e.ObexServer.MessageReceived += ObexServer_MessageReceived;
        }

        private async void ObexServer_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _logger.LogInformation(AppLogEvents.SmsServiceMessageReceived, "MnsServer received new message, handle: {MessageHandle}.", e.MessageHandle);
            Debug.Assert(_masClientSession != null);
            Debug.Assert(_masClientSession.ObexClient != null);
            BMessage message = await _masClientSession.ObexClient.GetMessageAsync(e.MessageHandle);
            _notificationService.ShowMessageNotification(message);
            _logger.LogTrace(AppLogEvents.SmsServiceMessageReceived, "Message {MessageHandle} body:\n{Body}", e.MessageHandle, message.Body);
        }

        public override void Dispose()
        {
            _masClientSession?.Dispose();
            _mnsServerSession.Dispose();

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

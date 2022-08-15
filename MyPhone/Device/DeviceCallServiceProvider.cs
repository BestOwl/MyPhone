using GoodTimeStudio.MyPhone.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone
{
    public class DeviceCallServiceProvider : BaseDeviceServiceProvider
    {
        public PhoneLineTransportDevice TransportDevice { get; }

        private Task? _taskInitPhoneLine;
        private PhoneLine? _selectedPhoneLine;

        private readonly ILogger<DeviceCallServiceProvider> _logger;

        public DeviceCallServiceProvider(BluetoothDevice bluetoothDevice, PhoneLineTransportDevice phoneLineTransportDevice, 
            ILogger<DeviceCallServiceProvider> logger)
            : base(bluetoothDevice)
        {
            TransportDevice = phoneLineTransportDevice;
            _logger = logger;
        }

        protected override async Task<bool> ConnectToServiceAsync()
        {
            _logger.LogInformation(AppLogEvents.CallServiceConnect, "Connecting to CallService");
            _logger.LogInformation(AppLogEvents.CallServiceConnect, "Requesting PhoneLineTransportDevice access from OS");
            var accessResult = await TransportDevice.RequestAccessAsync();
            if (accessResult == Windows.Devices.Enumeration.DeviceAccessStatus.Allowed)
            {
                _logger.LogInformation(AppLogEvents.CallServiceConnect, "PhoneLineTransportDevice access granted");
            }
            else
            {
                _logger.LogWarning(AppLogEvents.CallServiceConnect, "PhoneLineTransportDevice access denied, reason: {Reason}, contine anyway", accessResult);
            }

            if (!TransportDevice.IsRegistered())
            {
                _logger.LogInformation(AppLogEvents.CallServiceConnect, "PhoneLineTransportDevice not registered, registering.");
                TransportDevice.RegisterApp();
            }
            _logger.LogInformation(AppLogEvents.CallServiceConnect, "PhoneLineTransportDevice registered.");

            bool success = await TransportDevice.ConnectAsync();
            if (success)
            {
                _logger.LogInformation(AppLogEvents.CallServiceConnect, "CallService connected.");
                _logger.LogInformation(AppLogEvents.CallServiceConnect, "Initiate PhoneLine auto discovery.");
                _taskInitPhoneLine = InitPhoneLine();
            }
            return success;
        }

        protected override void OnDisconnected()
        {
            _logger.LogWarning(AppLogEvents.CallServiceDisconnect, "CallService disconnected.");
            base.OnDisconnected();
        }

        public async Task CallAsync(string phoneNumber)
        {
            if (_taskInitPhoneLine == null)
            {
                throw new InvalidOperationException("CallService has not been initialized.");
            }

            await _taskInitPhoneLine;
            if (_selectedPhoneLine == null || !_selectedPhoneLine.CanDial)
            {
                throw new OperationCanceledException();
            }
            // TODO: Lookup contacts book for displayName
            _selectedPhoneLine.Dial(phoneNumber, phoneNumber);
        }

        public async Task<PhoneLine?> GetSelectedPhoneLineAsync()
        {
            if (_taskInitPhoneLine != null)
            {
                await _taskInitPhoneLine;
                return _selectedPhoneLine;
            }

            return null;
        }

        #region Init PhoneLine
        public static async Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync()
        {
            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
            PhoneLineWatcher watcher = store.RequestLineWatcher();
            return watcher;
        }

        // https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.calls.phonelinewatcher?view=winrt-22000
        private async Task InitPhoneLine()
        {
            _logger.LogInformation(AppLogEvents.CallServicePhoneLineDiscovery, "Searching PhoneLine...");
            List<PhoneLine> phoneLinesAvailable = new List<PhoneLine>();
            var lineEnumerationCompletion = new TaskCompletionSource<bool>();

            PhoneLineWatcher phoneLineWatcher = await CreatePhoneLineWatcherAsync();
            _logger.LogInformation(AppLogEvents.CallServicePhoneLineDiscovery, "PhoneLineWatcher created");
            phoneLineWatcher.LineAdded += async (o, args) =>
            {
                phoneLinesAvailable.Add(await PhoneLine.FromIdAsync(args.LineId));
            };
            phoneLineWatcher.Stopped += (o, args) => lineEnumerationCompletion.TrySetResult(false);
            phoneLineWatcher.EnumerationCompleted += (o, args) => lineEnumerationCompletion.TrySetResult(true);
            _logger.LogInformation(AppLogEvents.CallServicePhoneLineDiscovery, "Start PhoneLineWatcher");
            phoneLineWatcher.Start();

            // Wait for enumeration completion
            _logger.LogInformation(AppLogEvents.CallServicePhoneLineDiscovery, "Wait for enumeration completion");
            if (await lineEnumerationCompletion.Task)
            {
                _logger.LogInformation(AppLogEvents.CallServicePhoneLineDiscovery, "PhoneLineWatcher enumeration completed.");
                _selectedPhoneLine = phoneLinesAvailable
                    .Where(pl => pl.TransportDeviceId == TransportDevice.DeviceId)
                    .FirstOrDefault();
            }
            else
            {
                _logger.LogWarning(AppLogEvents.CallServicePhoneLineDiscovery, "PhoneLineWatcher unexpectedly stopped.");
            }

            phoneLineWatcher.Stop();
            _logger.LogInformation(AppLogEvents.CallServicePhoneLineDiscovery, "PhoneLine auto discovery task completed. PhoneLineWatcher stopped");

        }
        #endregion Init PhoneLine
    }
}

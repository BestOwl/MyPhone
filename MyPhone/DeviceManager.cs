using GoodTimeStudio.MyPhone.Services;
using GoodTimeStudio.MyPhone.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone
{
    public class DeviceManager : IDisposable
    {
        private readonly IDeviceService _deviceService;

        private CurrentDeviceInformation? _currentDevice;
        private Task? _taskInitPhoneLine;
        private PhoneLine? _selectedPhoneLine;
        private DynamicTimer? _reconnectTimer;

        public DeviceManager(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Attempt to initialize the <see cref="DeviceManager"/> with previously registered device.
        /// </summary>
        /// <returns>Returns true if a previously registered device is found, false otherwise.</returns>
        public async Task<bool> InitAsync()
        {
            CurrentDeviceInformation? currentDevice = await _deviceService.GetCurrentRegisteredDeviceAsync();
            if (currentDevice == null)
            {
                return false;
            }

            _currentDevice = currentDevice;
            if (!await ReconnectAsync())
            {
                ScheduleReconnect();
            }
            return true;
        }

        static DynamicTimerSchedule[] s_schedules = new DynamicTimerSchedule[]
        {
            new DynamicTimerSchedule(TimeSpan.FromSeconds(15), 4), // T+1min
            new DynamicTimerSchedule(TimeSpan.FromSeconds(30), 10), // T+1min -> T+6min (5min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(1), 9), // T+6min -> T+15min (9min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(3), 5), // T+15min -> T+30min (15min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(5), 6), // T+30min -> T+60min (30min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(10), 6), // T+1h -> T+2h (1h in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(30), 2), // T+2h -> T+3h (1min in total)
            new DynamicTimerSchedule(TimeSpan.FromHours(1), 0), // T+3h -> 
        };

        /// <summary>
        /// Connect to a new phone device and register it, then initialize the <see cref="DeviceManager"/>
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> object obtained from <see cref="Windows.Devices.Enumeration"/> API</param>
        /// <returns>Whether connected to the device succesfully</returns>
        /// <remarks>
        /// The <see cref="DeviceInformation.Kind"/> must be <see cref="DeviceInformationKind.AssociationEndpoint"/> and it must be a Bluetooth device
        /// </remarks>
        /// <exception cref="DeviceParingException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        public async Task<bool> ConnectAsync(DeviceInformation deviceInformation)
        {
            if (deviceInformation == null)
            {
                throw new ArgumentNullException(nameof(deviceInformation));
            }
            if (deviceInformation.Kind != DeviceInformationKind.AssociationEndpoint)
            {
                throw new InvalidOperationException("Does not support this device");
            }

            if (!_deviceService.IsPaired(deviceInformation))
            {
                var paringResult = await _deviceService.PairDeviceAsync(deviceInformation);
                if (paringResult.Status != DevicePairingResultStatus.Paired)
                {
                    throw new DeviceParingException(paringResult);
                }
            }

            CurrentDeviceInformation? registeredDevice = await _deviceService.RegisterDeviceAsync(deviceInformation);
            if (registeredDevice == null)
            {
                return false;
            }

            _currentDevice = registeredDevice;
            _currentDevice.BluetoothDevice.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
            _taskInitPhoneLine = InitPhoneLine();
            return true;
        }

        public async Task<bool> ReconnectAsync()
        {
            if (_currentDevice == null)
            {
                throw new InvalidOperationException("DeviceManager has not been initialized.");
            }

            await _currentDevice.PhoneLineTransportDevice.RequestAccessAsync();
            if (!_currentDevice.PhoneLineTransportDevice.IsRegistered())
            {
                _currentDevice.PhoneLineTransportDevice.RegisterApp();
            }
            bool success = await _currentDevice.PhoneLineTransportDevice.ConnectAsync();
            if (success)
            {
                _taskInitPhoneLine = InitPhoneLine();
                _currentDevice.BluetoothDevice.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
            }
            return success;
        }

        private void ScheduleReconnect()
        {
            _reconnectTimer = new DynamicTimer(s_schedules);
            _reconnectTimer.Elapsed += _reconnectTimer_Elapsed;
            _reconnectTimer.Start();
        }

        private async void _reconnectTimer_Elapsed(object? sender, DynamicTimerElapsedEventArgs e)
        {
            if (await ReconnectAsync())
            {
                _reconnectTimer!.Stop();
            }
        }

        private void BluetoothDevice_ConnectionStatusChanged(Windows.Devices.Bluetooth.BluetoothDevice sender, object args)
        {
            if (sender.ConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Disconnected)
            {
                ScheduleReconnect();
            }
        }

        public void Dispose()
        {
            _reconnectTimer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task CallAsync(string phoneNumber)
        {
            if (_taskInitPhoneLine == null)
            {
                throw new InvalidOperationException("DeviceManager has not been initialized.");
            }

            await _taskInitPhoneLine;
            if (_selectedPhoneLine == null || !_selectedPhoneLine.CanDial)
            {
                throw new OperationCanceledException();
            }
            // TODO: Lookup contacts book for displayName
            _selectedPhoneLine.Dial(phoneNumber, phoneNumber);
        }

        #region Init PhoneLine
        // https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.calls.phonelinewatcher?view=winrt-22000
        private async Task InitPhoneLine()
        {
            Debug.Assert(_currentDevice != null);

            List<PhoneLine> phoneLinesAvailable = new List<PhoneLine>();
            var lineEnumerationCompletion = new TaskCompletionSource<bool>();

            PhoneLineWatcher phoneLineWatcher = await _deviceService.CreatePhoneLineWatcherAsync();
            phoneLineWatcher.LineAdded += async (o, args) =>
            {
                phoneLinesAvailable.Add(await PhoneLine.FromIdAsync(args.LineId));
            };
            phoneLineWatcher.Stopped += (o, args) => lineEnumerationCompletion.TrySetResult(false);
            phoneLineWatcher.EnumerationCompleted += (o, args) => lineEnumerationCompletion.TrySetResult(true);
            phoneLineWatcher.Start();

            // Wait for enumeration completion
            if (await lineEnumerationCompletion.Task)
            {
                _selectedPhoneLine = phoneLinesAvailable
                    .Where(pl => pl.TransportDeviceId == _currentDevice!.PhoneLineTransportDevice.DeviceId)
                    .FirstOrDefault();
            }

            phoneLineWatcher.Stop();
            Debug.WriteLine("PhoneLineWatcher stopped");

        }
        #endregion Init PhoneLine
    }

    public class DeviceParingException : Exception
    {
        public DevicePairingResult PairingResult { get; private set; }

        public DeviceParingException(DevicePairingResult result)
        {
            PairingResult = result;
        }

        public DeviceParingException(DevicePairingResult result, string? message) : base(message)
        {
            PairingResult = result;
        }

        public DeviceParingException(DevicePairingResult result, string? message, Exception? innerException) : base(message, innerException)
        {
            PairingResult = result;
        }
    }
}

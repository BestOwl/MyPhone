using GoodTimeStudio.MyPhone.Utilities;
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

        public DeviceCallServiceProvider(BluetoothDevice bluetoothDevice, PhoneLineTransportDevice phoneLineTransportDevice)
            : base(bluetoothDevice)
        {
            TransportDevice = phoneLineTransportDevice;
        }

        protected override async Task<bool> ConnectToServiceAsync()
        {
            await TransportDevice.RequestAccessAsync();
            if (!TransportDevice.IsRegistered())
            {
                TransportDevice.RegisterApp();
            }
            bool success = await TransportDevice.ConnectAsync();
            if (success)
            {
                _taskInitPhoneLine = InitPhoneLine();
            }
            return success;
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
            List<PhoneLine> phoneLinesAvailable = new List<PhoneLine>();
            var lineEnumerationCompletion = new TaskCompletionSource<bool>();

            PhoneLineWatcher phoneLineWatcher = await CreatePhoneLineWatcherAsync();
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
                    .Where(pl => pl.TransportDeviceId == TransportDevice.DeviceId)
                    .FirstOrDefault();
            }

            phoneLineWatcher.Stop();
            Debug.WriteLine("PhoneLineWatcher stopped");

        }
        #endregion Init PhoneLine
    }
}

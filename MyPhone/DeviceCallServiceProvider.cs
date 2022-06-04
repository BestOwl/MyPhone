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
        private PhoneLineTransportDevice _transportDevice;

        private Task? _taskInitPhoneLine;
        private PhoneLine? _selectedPhoneLine;

        public DeviceCallServiceProvider(BluetoothDevice bluetoothDevice, PhoneLineTransportDevice phoneLineTransportDevice)
            : base(bluetoothDevice)
        {
            _transportDevice = phoneLineTransportDevice;
        }

        protected override async Task<bool> ConnectToServiceAsync()
        {
            await _transportDevice.RequestAccessAsync();
            if (!_transportDevice.IsRegistered())
            {
                _transportDevice.RegisterApp();
            }
            bool success = await _transportDevice.ConnectAsync();
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

        #region Init PhoneLine
        private static async Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync()
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
                    .Where(pl => pl.TransportDeviceId == _transportDevice.DeviceId)
                    .FirstOrDefault();
            }

            phoneLineWatcher.Stop();
            Debug.WriteLine("PhoneLineWatcher stopped");

        }
        #endregion Init PhoneLine
    }
}

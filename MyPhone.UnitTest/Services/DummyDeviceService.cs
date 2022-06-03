using GoodTimeStudio.MyPhone.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;

namespace MyPhone.UnitTest.Services
{
    internal class DummyDeviceService : IDeviceService
    {
        public DeviceWatcher CreateDeviceWatcher()
        {
            throw new NotImplementedException();
        }

        public Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CurrentDeviceInformation?> GetCurrentRegisteredDeviceAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsPaired(DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
        }

        public Task<DevicePairingResult> PairDeviceAsync(DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
        }

        public Task<CurrentDeviceInformation?> RegisterDeviceAsync(DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
        }
    }
}

using GoodTimeStudio.MyPhone.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;

namespace MyPhone.UnitTest.Services
{
    internal class DummyDeviceService : IDeviceService
    {
        public Task CallAsync(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectAsync(DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
        }

        public DeviceWatcher CreateDeviceWatcher()
        {
            throw new NotImplementedException();
        }

        public Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DeviceInformation?> GetCurrentDeviceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReconnectAsync()
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Extensions.Configuration;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class BluetoothDeviceFixture
    {
        public string BluetoothDeviceId { get; }

        public IConfigurationRoot Configuration { get; }

        public BluetoothDeviceFixture()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("devicetest.local.json").Build();
            BluetoothDeviceId = Configuration["deviceId"];
            Console.WriteLine($"Current test device ID: {BluetoothDeviceId}");
        }
    }
}

using Microsoft.Extensions.Configuration;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class BluetoothDeviceFixture
    {
        public BluetoothDevice BluetoothDevice { get; }

        public BluetoothDeviceFixture()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("devicetest.local.json").Build();
            BluetoothDevice = BluetoothDevice.FromIdAsync(configuration["deviceId"]).GetResults();
            Console.WriteLine($"Current test device: {BluetoothDevice.Name}");
        }
    }
}

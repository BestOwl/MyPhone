using Microsoft.Extensions.Configuration;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class BluetoothDeviceFixture : IAsyncLifetime
    {
        private BluetoothDevice? _device;
        public BluetoothDevice BluetoothDevice { get => _device ?? throw new InvalidOperationException(); }

        private IConfigurationRoot? _config;
        public IConfigurationRoot Configuration { get => _config ?? throw new InvalidOperationException(); }

        public async Task InitializeAsync()
        {
            _config = new ConfigurationBuilder().AddJsonFile("devicetest.local.json").Build();
            _device = await BluetoothDevice.FromIdAsync(_config["deviceId"]);
            Console.WriteLine($"Current test device: {BluetoothDevice.Name}");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class BluetoothDeviceFixture : IAsyncLifetime
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public BluetoothDevice BluetoothDevice { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public async Task InitializeAsync()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("devicetest.local.json").Build();
            BluetoothDevice = await BluetoothDevice.FromIdAsync(configuration["deviceId"]);
            Console.WriteLine($"Current test device: {BluetoothDevice.Name}");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

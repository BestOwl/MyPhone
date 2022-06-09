using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Helpers;
using GoodTimeStudio.MyPhone.Services;
using GoodTimeStudio.MyPhone.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace GoodTimeStudio.MyPhone.Device
{
    public class DeviceManager : IDisposable
    {
        private bool _started;

        public BluetoothDevice CurrentDevice { get; private set; }
        public DeviceCallServiceProvider? CallService { get; private set; }
        public DeviceSmsServiceProvider? SmsService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve device-specific services.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Construct a new instance of <see cref="DeviceManager"/>
        /// </summary>
        /// <param name="bluetoothDevice">
        /// The remote Bluetooth device that have already been paired with this system. 
        /// The operating system must have granted access to the remote Bluetooth device.
        /// </param>
        public DeviceManager(BluetoothDevice bluetoothDevice)
        {
            _started = false;
            CurrentDevice = bluetoothDevice;
            Services = ConfigureDeviceServices().BuildServiceProvider();
        }

        private IServiceCollection ConfigureDeviceServices()
        {
            string dbPath = Path.Join(
                ApplicationData.Current.LocalFolder.Path, "DeviceData", CurrentDevice.BluetoothAddress.ToHexString());
            return new ServiceCollection()
                .AddDbContext<DeviceDbContext>(options => options.UseSqlite($"Data Source={dbPath}"))
                .AddEntityFrameworkMessageStore();
        }

        /// <summary>
        /// Start trying to connect to the remote Bluetooth device and start all device services.
        /// </summary>
        public async Task StartAsync()
        {
            if (_started)
            {
                throw new InvalidOperationException("DeviceManager is already started");
            }
            await InitializeDeviceServices();
            _ = ConnectAllDeviceServices();
            _started = true;
        }

        private async Task InitializeDeviceServices()
        {
            PhoneLineTransportDevice? phoneLineTransportDevice = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(CurrentDevice);
            if (phoneLineTransportDevice != null)
            {
                DeviceAccessStatus accessStatus = await phoneLineTransportDevice.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    CallService = new DeviceCallServiceProvider(CurrentDevice, phoneLineTransportDevice);
                }
            }
            SmsService = new DeviceSmsServiceProvider(CurrentDevice);
        }

        /// <summary>
        /// Connect to all device services.
        /// </summary>
        /// <returns>Whether all device service is connected successfully</returns>
        /// <exception cref="InvalidOperationException">Throws if the client call this method in an invalid state</exception>
        private async Task<bool> ConnectAllDeviceServices()
        {
            if (CurrentDevice == null)
            {
                throw new InvalidOperationException("You must first call ConnectAsync or TryReconnect (return true).");
            }

            bool connected = true;
            if (CallService != null)
            {
                connected = await CallService.ConnectAsync() && connected;
            }
            connected = await SmsService!.ConnectAsync() && connected;
            return connected;
        }

        public void Dispose()
        {
            CallService?.Dispose();
            SmsService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

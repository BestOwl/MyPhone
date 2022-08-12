using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Helpers;
using GoodTimeStudio.MyPhone.Services;
using GoodTimeStudio.MyPhone.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<DeviceManager> _logger;

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
            _logger = App.Current.Services.GetRequiredService<ILogger<DeviceManager>>();
            Services = ConfigureDeviceServices().BuildServiceProvider();
            using (var scope = Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DeviceDbContext>();
                context.Database.EnsureCreated();
            }
        }

        private IServiceCollection ConfigureDeviceServices()
        {
            FileInfo dbFile = new FileInfo(
                Path.Join(
                    ApplicationData.Current.LocalFolder.Path, 
                    "DeviceData", 
                    CurrentDevice.BluetoothAddress.ToHexString() + ".db")
                );
            return new ServiceCollection()
                .AddDbContext<DeviceDbContext>(options => options.UseSqlite($"Data Source=\"{dbFile.FullName}\""))
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
            _logger.LogInformation("Initializing device services.");
            await InitializeDeviceServices();
            _logger.LogInformation("Device services initialized.");
            _ = ConnectAllDeviceServices();
            _started = true;
        }

        private async Task InitializeDeviceServices()
        {
            #region Init CallService
            PhoneLineTransportDevice? phoneLineTransportDevice = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(CurrentDevice);
            if (phoneLineTransportDevice != null)
            {
                _logger.LogInformation("Requesting PhoneLineTransportDeivce access.");
                DeviceAccessStatus accessStatus = await phoneLineTransportDevice.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    _logger.LogInformation("PhoneLineTransportDeivce access granted.");
                    CallService = new DeviceCallServiceProvider(CurrentDevice, phoneLineTransportDevice, 
                        App.Current.Services.GetRequiredService<ILogger<DeviceCallServiceProvider>>());
                    _logger.LogInformation("CallService initialized.");
                }
                else
                {
                    _logger.LogWarning("PhoneLineTransportDevice access denied, skipping CallService.");
                }
            }
            else
            {
                _logger.LogWarning("PhoneLineTransportDevice not available, skip connecting CallService");
            }
            #endregion

            SmsService = new DeviceSmsServiceProvider(CurrentDevice, 
                App.Current.Services.GetRequiredService<ILogger<DeviceSmsServiceProvider>>(),
                App.Current.Services.GetRequiredService<IMessageNotificationService>(),
                Services.GetRequiredService<IMessageStore>(),
                App.Current.Services.GetRequiredService<ILogger<MessageSynchronizer>>());
            _logger.LogInformation("SmsService initialized.");
        }

        /// <summary>
        /// Connect to all device services.
        /// </summary>
        /// <returns>Whether all device service is connected successfully</returns>
        /// <exception cref="InvalidOperationException">Throws if the client call this method in an invalid state</exception>
        private async Task<bool> ConnectAllDeviceServices()
        {
            _logger.LogInformation("Connecting to device services.");
            if (CurrentDevice == null)
            {
                throw new InvalidOperationException("You must first call ConnectAsync or TryReconnect (return true).");
            }

            bool connected = true;
            if (CallService != null)
            {
                _logger.LogInformation("Connecting to CallService.");
                connected = await CallService.ConnectAsync() && connected;
                if (connected)
                {
                    _logger.LogInformation("CallService connected.");
                }
                else
                {
                    _logger.LogWarning("Unable to connect CallService.");
                }
            }
            else
            {
                _logger.LogWarning("CallService not available, skipping.");
            }

            _logger.LogInformation("Connecting to SmsService.");
            connected = await SmsService!.ConnectAsync() && connected;
            if (connected)
            {
                _logger.LogInformation("SmsService connected.");
            }
            else
            {
                _logger.LogWarning("Unable to connect SmsService, state: {State}.", SmsService.State);
                if (SmsService.StopReason != null)
                {
                    _logger.LogWarning(SmsService.StopReason, "SmsService stopped.");
                }
            }

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

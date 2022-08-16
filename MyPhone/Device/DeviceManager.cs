using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Device.Services;
using GoodTimeStudio.MyPhone.Extensions;
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
        public DevicePhonebookServiceProvider? PhonebookService { get; private set; }

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
            FileInfo dbFile = new FileInfo(
                Path.Join(
                    ApplicationData.Current.LocalFolder.Path,
                    "DeviceData",
                    CurrentDevice.BluetoothAddress.ToHexString() + ".db")
                );
            _logger.LogInformation("DeviceDatabasePath: {DbPath}", dbFile.FullName);
            if (dbFile.Directory == null)
            {
                throw new InvalidOperationException("Device database directory should not be null.");
            }
            if (!dbFile.Directory.Exists) 
            {
                _logger.LogInformation("The directory of DeviceDatabasePath does not exists, creating.");
                dbFile.Directory.Create();
                _logger.LogInformation("The directory of DeviceDatabasePath created successfully.");
            }
            Services = ConfigureDeviceServices(dbFile).BuildServiceProvider();
            using (var scope = Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DeviceDbContext>();
                context.Database.EnsureCreated();
            }
            Services.GetRequiredService<IDeviceConfiguration>().DeviceId = bluetoothDevice.DeviceId;
        }

        private static IServiceCollection ConfigureDeviceServices(FileInfo deviceDatabaseFile)
        {
            return new ServiceCollection()
                .AddDbContext<DeviceDbContext>(options => options.UseSqlite($"Data Source=\"{deviceDatabaseFile.FullName}\""))
                .AddEntityFrameworkDeviceConfiguration()
                .AddEntityFrameworkMessageStore()
                .AddEntityFrameworkContactStore();
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
                Services.GetRequiredService<IMessageStore>());
            _logger.LogInformation("SmsService initialized.");

            PhonebookService = new DevicePhonebookServiceProvider(CurrentDevice,
                Services.GetRequiredService<IContactStore>(),
                Services.GetRequiredService<IDeviceConfiguration>(),
                App.Current.Services.GetRequiredService<ILogger<DevicePhonebookServiceProvider>>());
            _logger.LogInformation("PhonebookService initialized.");
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

            bool callServiceConnected = false;
            if (CallService != null)
            {
                _logger.LogInformation("Connecting to CallService.");
                callServiceConnected = await CallService.ConnectAsync();
                if (callServiceConnected)
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
            bool smsServiceConnected = await SmsService!.ConnectAsync();
            if (smsServiceConnected)
            {
                _logger.LogInformation("SmsService connected.");
            }
            else
            {
                _logger.LogWarning("Unable to connect SmsService, state: {State}.", SmsService.State);
                if (SmsService.StopReason != null)
                {
                    _logger.LogWarning(SmsService.StopReason, "SmsService stopped because of exceptions.");
                }
            }

            _logger.LogInformation("Connecting to PhonebookService.");
            bool phonebookServiceConnected = await PhonebookService!.ConnectAsync();
            if (phonebookServiceConnected)
            {
                _logger.LogInformation("PhonebookService connected.");
            }
            else
            {
                _logger.LogWarning("Unable to connect PhonebookService, state: {State.}", PhonebookService.State);
                if (PhonebookService.StopReason != null)
                {
                    _logger.LogWarning(PhonebookService.StopReason, "PhonebookService stopped because of exceptions.");
                }
            }

            return callServiceConnected && smsServiceConnected && phonebookServiceConnected;
        }

        public void Dispose()
        {
            CallService?.Dispose();
            SmsService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

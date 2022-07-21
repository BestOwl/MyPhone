using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Notifications;
using GoodTimeStudio.MyPhone.Device;
using GoodTimeStudio.MyPhone.Models;
using GoodTimeStudio.MyPhone.Pages;
using GoodTimeStudio.MyPhone.RootPages ;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Shiny;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        public IConfiguration Configuration { get; private set; }

        public DeviceManager? DeviceManager { get; private set; }

        private readonly IDevicePairingService _devicePairingService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<App> _logger;

        private Window? m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<App>()
                .Build();

            Services = ConfigureServices().BuildServiceProvider();

            _devicePairingService = Services.GetRequiredService<IDevicePairingService>();
            _settingsService = Services.GetRequiredService<ISettingsService>();
            _logger = Services.GetRequiredService<ILogger<App>>();

            string? appCenterSecrets = Configuration["ApiSecrets:MsftAppCenter"];
            if (appCenterSecrets != null)
            {
                _logger.LogInformation("Found App Center secret, starting App Center.");
                AppCenter.LogLevel = Microsoft.AppCenter.LogLevel.Verbose;
                AppCenter.Start(appCenterSecrets,
                  typeof(Analytics), typeof(Crashes));
            }
            else
            {
                _logger.LogWarning("App Center secret not found, skip App Center initialization.");
            }

            _logger.LogInformation(AppLogEvents.AppLaunch, "App instance created.");
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _logger.LogInformation(AppLogEvents.AppLaunch, "Launching My Phone App.");

            AppInstance currentInstance = AppInstance.GetCurrent();
            AppActivationArguments activationArgs = currentInstance.GetActivatedEventArgs();
            currentInstance.Activated += MainInstance_RedirectedActivated;
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (m_window == null)
            {
                _logger.LogInformation(AppLogEvents.AppLaunch, "Initializing App.");
                await InitApp();
                _logger.LogInformation(AppLogEvents.AppLaunch, "App initialized");

                _logger.LogInformation(AppLogEvents.AppLaunch, "Initializing MainWindow");
                m_window = Services.GetRequiredService<MainWindow>();
                _logger.LogInformation(AppLogEvents.AppLaunch, "MainWindow initialized.");
            }
            else
            {
                _logger.LogInformation(AppLogEvents.AppLaunch, "App and MainWindow already initialized, skip initialization.");
            }

            if (activationArgs.Kind != ExtendedActivationKind.StartupTask)
            {
                _logger.LogInformation(AppLogEvents.AppLaunch, "Activate MainWindow.");
                m_window.Activate();
            }
            else
            {
                _logger.LogInformation(AppLogEvents.AppLaunch, "App launched by StartupTask, skip activating MainWindow");
            }
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            // TODO: activate main window and navigate to content
            throw new NotImplementedException();
        }

        // Handle redirected OnActivated
        private void MainInstance_RedirectedActivated(object? sender, AppActivationArguments e)
        {
            _logger.LogInformation(AppLogEvents.AppLaunch, "App instance redirect activated.");
            m_window!.DispatcherQueue.TryEnqueue(() =>
            {
                _logger.LogInformation(AppLogEvents.AppLaunch, "Activate MainWindow.");
                m_window.Activate();
            });
        }

        private async Task InitApp()
        {
            _logger.LogInformation(AppLogEvents.AppLaunch, "Try to find and setup registered device.");
            await TryAutoSetupRegisteredDevice();
        }

        /// <summary>
        /// Attempt to setup to a previously registered device.
        /// </summary>
        private async Task TryAutoSetupRegisteredDevice()
        {
            string? deviceId = _settingsService.GetValue<string>(_settingsService.KeyCurrentDeviceId);
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogInformation(AppLogEvents.AppLaunch, "No registered device found");
                return;
            }

            DeviceInformation deviceInformation = await DeviceInformation.CreateFromIdAsync(deviceId);
            _logger.LogInformation(AppLogEvents.AppLaunch, "Found registered device: {DeviceName}", deviceInformation.Name);
            try
            {
                await SetupDevice(deviceInformation, registerDevice: false);
            }
            catch (DevicePairingException ex)
            {
                _logger.LogWarning(AppLogEvents.DeviceSetup, "Failed to auto setup device {DeviceName}. {ExMessage}", deviceInformation.Name, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(AppLogEvents.DeviceSetup, "Failed to auto setup device {DeviceName}. {ExMessage}", deviceInformation.Name, ex.Message);
            }
        }

        /// <summary>
        /// Setup and register a device
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> of a device, obtained from <see cref="Windows.Devices.Enumeration"/> API</param>
        /// <param name="registerDevice">Whether to register the device so that it can be auto setup during next app launch</param>
        /// <remarks>
        /// The <see cref="DeviceInformation.Kind"/> must be <see cref="DeviceInformationKind.AssociationEndpoint"/> and it must be a Bluetooth device
        /// </remarks>
        /// <exception cref="DevicePairingException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        public async Task SetupDevice(DeviceInformation deviceInformation, bool registerDevice = true)
        {
            _logger.LogInformation(AppLogEvents.DeviceSetup, "Setting up device: {DeviceName}", deviceInformation.Name);
            if (deviceInformation == null)
            {
                throw new ArgumentNullException(nameof(deviceInformation));
            }
            if (deviceInformation.Kind != DeviceInformationKind.AssociationEndpoint)
            {
                throw new InvalidOperationException("Does not support this device");
            }

            if (!_devicePairingService.IsPaired(deviceInformation))
            {
                _logger.LogInformation(AppLogEvents.DeviceSetup, "Device not paired, attempting to pair");
                var paringResult = await _devicePairingService.PairDeviceAsync(deviceInformation);
                if (paringResult.Status != DevicePairingResultStatus.Paired)
                {
                    throw new DevicePairingException(paringResult);
                }
                _logger.LogInformation(AppLogEvents.DeviceSetup, "Device {DeviceName} paired successfully.", deviceInformation.Name);
            }

            BluetoothDevice bluetoothDevice = await BluetoothDevice.FromIdAsync(deviceInformation.Id);
            _logger.LogInformation(AppLogEvents.DeviceSetup, "Requesting BluetoothDevice access.");
            DeviceAccessStatus status = await bluetoothDevice.RequestAccessAsync();
            if (status != DeviceAccessStatus.Allowed)
            {
                throw new UnauthorizedAccessException($"The operating system denied the access to this BluetoothDevice. Reason: {status}");
            }
            else
            {
                _logger.LogInformation(AppLogEvents.DeviceSetup, "BluetoothDevice access granted.");
            }

            if (registerDevice)
            {
                _logger.LogInformation(AppLogEvents.DeviceSetup, "Registering device: {DeviceName}", deviceInformation.Name);
                _settingsService.SetValue(_settingsService.KeyCurrentDeviceId, deviceInformation.Id);
                _settingsService.SetValue(_settingsService.KeyOobeIsCompleted, true);
                _logger.LogInformation("Device registered successfully.");
            }
            else
            {
                _logger.LogInformation(AppLogEvents.DeviceSetup, "Skip device registration.");
            }

            _logger.LogInformation(AppLogEvents.DeviceSetup, "Initializing DeviceManager.");
            DeviceManager = new DeviceManager(bluetoothDevice);
            _logger.LogInformation(AppLogEvents.DeviceSetup, "DeviceManager initialzed.");
            _logger.LogInformation(AppLogEvents.DeviceSetup, "Starting DeviceManager services.");

            await DeviceManager.StartAsync();

            _logger.LogInformation(AppLogEvents.DeviceSetup, "Device setup successfully.");
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceCollection ConfigureServices()
        {
            // Register services
            return new ServiceCollection()
                .AddLogging(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                    loggerBuilder.AddDebug();
                    loggerBuilder.AddAppCenter();
                })
                .AddAppDataLocalSettings()
                .AddMessageToastNotification()
                .AddDevicePairingService()
                .AddDevicePairDialog()
                .AddTransient<MainWindow>()
                .AddTransient<OobePageViewModel>();
        }
    }
}

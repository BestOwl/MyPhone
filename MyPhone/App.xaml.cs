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
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;


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
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Single-instance redirect, redirect Activated to the main instance 
            AppInstance mainInstance = AppInstance.FindOrRegisterForKey("main");
            AppInstance currentInstance = AppInstance.GetCurrent();
            AppActivationArguments activationArgs = currentInstance.GetActivatedEventArgs();
            if (mainInstance != currentInstance)
            {
                await mainInstance.RedirectActivationToAsync(activationArgs);
                Process.GetCurrentProcess().Kill();
                return;
            }
            mainInstance.Activated += MainInstance_RedirectedActivated;
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;

            string? appCenterSecrets = Configuration["ApiSecrets:MsftAppCenter"];
            if (appCenterSecrets != null)
            {
                AppCenter.Start(appCenterSecrets, typeof(Analytics), typeof(Crashes));
            }

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (m_window == null)
            {
                await  InitApp();
                m_window = Services.GetRequiredService<MainWindow>();
            }

            if (activationArgs.Kind != ExtendedActivationKind.StartupTask)
            {
                m_window.Activate();
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
            m_window!.DispatcherQueue.TryEnqueue(() =>
            {
                m_window.Activate();
            });
        }

        private async Task InitApp()
        {
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
                return;
            }

            DeviceInformation deviceInformation = await DeviceInformation.CreateFromIdAsync(deviceId);
            try
            {
                await SetupDevice(deviceInformation, registerDevice: false);
            }
            catch (DevicePairingException ex)
            {
                Console.WriteLine($"Failed to auto setup device. {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Failed to auto setup device. {ex.Message}");
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
                var paringResult = await _devicePairingService.PairDeviceAsync(deviceInformation);
                if (paringResult.Status != DevicePairingResultStatus.Paired)
                {
                    throw new DevicePairingException(paringResult);
                }
            }

            BluetoothDevice bluetoothDevice = await BluetoothDevice.FromIdAsync(deviceInformation.Id);
            DeviceAccessStatus status = await bluetoothDevice.RequestAccessAsync();
            if (status != DeviceAccessStatus.Allowed)
            {
                throw new UnauthorizedAccessException($"The operating system denied the access to this Bluetooth device. Reason: {status}");
            }

            if (registerDevice)
            {
                _settingsService.SetValue(_settingsService.KeyCurrentDeviceId, deviceInformation.Id);
                _settingsService.SetValue(_settingsService.KeyOobeIsCompleted, true);
            }
            DeviceManager = new DeviceManager(bluetoothDevice);
            await DeviceManager.StartAsync();
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceCollection ConfigureServices()
        {
            // Register services
            return new ServiceCollection()
                .AddAppDataLocalSettings()
                .AddMessageToastNotification()
                .AddDevicePairingService()
                .AddDevicePairDialog()
                .AddTransient<MainWindow>();
        }
    }
}

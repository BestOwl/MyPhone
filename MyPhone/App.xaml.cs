using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Notifications;
using GoodTimeStudio.MyPhone.Pages;
using GoodTimeStudio.MyPhone.RootPages ;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
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

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (m_window == null)
            {
                ConfigureServices();
                m_window = Ioc.Default.GetRequiredService<MainWindow>();
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

        private static void ConfigureServices()
        {
            // Register services
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddAppDataLocalSettings()
                .AddMessageToastNotification()
                .AddDevicePairingService()
                .AddDevicePairDialog()
                .AddSingleton<DeviceManager>()
                .AddTransient<OobePageViewModel>()
                .AddTransient<MainWindow>()
                .AddTransient<CallPageViewModel>()
                .AddTransient<DiagnosisPageViewModel>()
                .BuildServiceProvider());
        }
    }
}

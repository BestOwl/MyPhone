using GoodTimeStudio.MyPhone.RootPages.Main;
using GoodTimeStudio.MyPhone.RootPages.OOBE;
using GoodTimeStudio.MyPhone.Services;
using H.NotifyIcon;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Win32;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public string AppTitleDisplayName { get => Windows.ApplicationModel.Package.Current.DisplayName; }
        public XamlRoot XamlRoot { get => windowRoot.XamlRoot; }

        private readonly IDeviceService deviceService;
        private readonly ISettingsService settingsService;
        private bool OobeCompleted;
        private IntPtr hWnd;

        public MainWindow(IDeviceService deviceService, ISettingsService settingsService)
        {
            Instance = this;
            this.InitializeComponent();
            this.deviceService = deviceService;
            this.settingsService = settingsService;
            Title = AppTitleDisplayName;
            OobeCompleted = settingsService.GetValue<bool>(settingsService.KeyOobeIsCompleted);


            // Hide default title bar.
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar); // Set XAML element as a draggable region.

            //Register a handler for when the window changes focus
            Activated += MainWindow_Activated;

            // AppWindow Interop
            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Closing += AppWindow_Closing;

            // Resize the window 
            uint dpi = PInvoke.GetDpiForWindow((Windows.Win32.Foundation.HWND)hWnd);
            double factor = dpi / 96d;
            appWindow.Resize(new Windows.Graphics.SizeInt32(Convert.ToInt32(1200 * factor), Convert.ToInt32(800 * factor)));


            OnLaunch();
        }

        private async void OnLaunch()
        {
            var currentDev = await deviceService.GetCurrentDeviceAsync();
            if (OobeCompleted && currentDev != null)
            {
                _ = deviceService.ReconnectAsync();
                rootFrame.Navigate(typeof(MainPage));
            }
            else
            {
                rootFrame.Navigate(typeof(OobePage));
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["WindowCaptionForeground"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["WindowCaptionForegroundDisabled"];

            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                AppTitle.Foreground = inactiveForegroundBrush;
            }
            else
            {
                AppTitle.Foreground = defaultForegroundBrush;
            }
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            this.Hide(enableEfficiencyMode: true);
            args.Cancel = true;
        }

        private void OpenAppCommand_ExecuteRequested(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args)
        {
            this.Show(disableEfficiencyMode: true);
            PInvoke.SetForegroundWindow((Windows.Win32.Foundation.HWND)hWnd);
        }

        private void ExitCommand_ExecuteRequested(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args)
        {
            Close();
        }
    }
}

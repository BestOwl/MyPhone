using CommunityToolkit.Mvvm.DependencyInjection;
using GoodTimeStudio.MyPhone.Pages.Call;
using GoodTimeStudio.MyPhone.Pages.Diagnosis;
using GoodTimeStudio.MyPhone.Pages.Message;
using GoodTimeStudio.MyPhone.RootPages.Main;
using GoodTimeStudio.MyPhone.RootPages.OOBE;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
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
        public readonly IntPtr WindowHandle;
        public XamlRoot XamlRoot { get => windowRoot.XamlRoot; }

        private NotifyIcon NotifyIcon;
        private DestroyIconSafeHandle _hIcon; // TODO: destory icon on exit

        private readonly ISettingsService SettingsService;
        private bool OobeCompleted;

        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();
            SettingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            Title = AppTitleDisplayName;
            OobeCompleted = SettingsService.GetValue<bool>(SettingsService.KeyOobeIsCompleted);


            // Hide default title bar.
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar); // Set XAML element as a draggable region.

            //Register a handler for when the window changes focus
            Activated += MainWindow_Activated;


            // Retrieve the window handle (HWND) of the current (XAML) WinUI 3 window.
            WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            // Retrieve the WindowId that corresponds to hWnd.
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(WindowHandle);
            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Closing += AppWindow_Closing;

            // Resize the window 
            uint dpi = PInvoke.GetDpiForWindow((Windows.Win32.Foundation.HWND)WindowHandle);
            double factor = dpi / 96d;
            appWindow.Resize(new Windows.Graphics.SizeInt32(Convert.ToInt32(1200 * factor), Convert.ToInt32(800 * factor)));

            // Get embeded icon from exe
            string? exePath = Environment.ProcessPath;
            if (exePath == null)
            {
                throw new PlatformNotSupportedException("Could not get the process exe path");
            }
            FreeLibrarySafeHandle hInstance = PInvoke.GetModuleHandle((string?)null); 
            _hIcon = PInvoke.ExtractIcon(hInstance, exePath, 0);
            if (_hIcon.IsInvalid || _hIcon.IsClosed)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // Set Window icon via old-school Win32 API as WinUI does not provide such API as of Windows App SDK 1.0
            IntPtr unsafe_hIcon = _hIcon.DangerousGetHandle();
            const uint WM_SETICON = 0x0080;
            PInvoke.SendMessage(
                (Windows.Win32.Foundation.HWND)WindowHandle,
                WM_SETICON, 
                new Windows.Win32.Foundation.WPARAM(0),
                new Windows.Win32.Foundation.LPARAM(unsafe_hIcon)
            );


            NotifyIcon = new NotifyIcon(WindowHandle, unsafe_hIcon);

            if (OobeCompleted)
            {
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
            // TODO: hide to system tray
            args.Cancel = true;
        }
    }
}

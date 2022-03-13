using GoodTimeStudio.MyPhone.Pages.Call;
using GoodTimeStudio.MyPhone.Pages.Diagnosis;
using GoodTimeStudio.MyPhone.Pages.Message;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public string AppTitleDisplayName { get => Windows.ApplicationModel.Package.Current.DisplayName; }

        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();

            // Hide default title bar.
            ExtendsContentIntoTitleBar = true;

            // Set XAML element as a draggable region.
            //SetTitleBar(AppTitleBar);

            //Register a handler for when the window changes focus
            Activated += MainWindow_Activated;

            #region HWND interop

            // Retrieve the window handle (HWND) of the current (XAML) WinUI 3 window.
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Retrieve the WindowId that corresponds to hWnd.
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Closing += AppWindow_Closing;
            #endregion

            ViewModel = new MainWindowViewModel();
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // TODO: hide to system tray
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

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {

            }
            else
            {
                NavigationMenu item = (NavigationMenu)args.SelectedItem; //TODO: better MVVM
                switch (item.Name)
                {
                    case "Call":
                        contentFrame.Navigate(typeof(CallPage));
                        break;
                    case "Message":
                        contentFrame.Navigate(typeof(MessagePage));
                        break;
                    case "Debug":
                        contentFrame.Navigate(typeof(DiagnosisPage));
                        break;
                }
            }
        }
    }
}

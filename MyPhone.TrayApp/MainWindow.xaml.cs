using Microsoft.Toolkit.Wpf.UI.XamlHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Windows.UI.Xaml.Controls;
using Style = Windows.UI.Xaml.Style;
using Setter = Windows.UI.Xaml.Setter;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Controls.Primitives;

namespace GoodTimeStudio.MyPhone.TrayApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MenuFlyout _ContextMenu;
        Grid _Root;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void _Host_ChildChanged(object sender, EventArgs e)
        {
            WindowsXamlHost windowsXamlHost = (WindowsXamlHost)sender;
            _Root = (Grid)windowsXamlHost.Child;
            //_Root.Background = null;

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Hello world! Xaml Island!";
            //_Root.Children.Add(textBlock);

            _ContextMenu = new MenuFlyout();
            _ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Exit", Icon = new SymbolIcon(Symbol.Clear) });

            //Style style = new Style(typeof(MenuFlyoutPresenter));
            //style.Setters.Add(new Setter(Windows.UI.Xaml.FrameworkElement.MinWidthProperty, 150));
            //_ContextMenu.MenuFlyoutPresenterStyle = style;

        }

        private void _NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            //POINT point;
            //GetCursorPos(out point);
            //this.Left = point.X;
            //this.Top = point.Y;
            _ContextMenu.ShowAt(_Root);

            e.Handled = true;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);

        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private void _Window_Deactivated(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Close");
        }
    }
}

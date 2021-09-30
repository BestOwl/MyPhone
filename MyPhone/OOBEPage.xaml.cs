using GoodTimeStudio.MyPhone.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class OOBEPage : Page
    {
        public OOBEPageViewModel ViewModel;
        public BluetoothDeviceListViewModel DListModel;

        MenuFlyout _ContextMenu;

        public OOBEPage()
        {
            this.InitializeComponent();
            DListModel = _List.ViewModel;
            ViewModel = new OOBEPageViewModel(DListModel);

            _ContextMenu = new MenuFlyout();
            _ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Exit", Icon = new SymbolIcon(Symbol.Clear) });

            Style style = new Style(typeof(MenuFlyoutPresenter));
            style.Setters.Add(new Setter(Windows.UI.Xaml.FrameworkElement.MinWidthProperty, 150));
            _ContextMenu.MenuFlyoutPresenterStyle = style;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DListModel.DeviceScanStart();
        }

        private async void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.Connect();
        }
    }
}

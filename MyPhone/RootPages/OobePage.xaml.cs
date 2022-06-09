using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GoodTimeStudio.MyPhone.RootPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class OobePage : Page
    {
        public OobePageViewModel ViewModel;

        public OobePage()
        {
            InitializeComponent();
            ViewModel = new OobePageViewModel();
            ViewModel.OobeCompletedEvent += ViewModel_OobeCompletedEvent;
        }

        private void ViewModel_OobeCompletedEvent(object? sender, EventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            bluetoothDeviceList.DeviceScanStart();
        }
    }
}

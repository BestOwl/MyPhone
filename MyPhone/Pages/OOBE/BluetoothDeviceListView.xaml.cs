using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace GoodTimeStudio.MyPhone.Pages.OOBE
{
    public sealed partial class BluetoothDeviceListView : UserControl
    {
        public BluetoothDeviceListViewModel ViewModel;

        public BluetoothDeviceListView()
        {
            this.InitializeComponent();
            ViewModel = new BluetoothDeviceListViewModel();
        }

    }
}

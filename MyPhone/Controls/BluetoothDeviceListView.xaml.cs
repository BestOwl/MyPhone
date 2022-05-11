using GoodTimeStudio.MyPhone.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace GoodTimeStudio.MyPhone.Controls
{
    public sealed partial class BluetoothDeviceListView : UserControl
    {
        private BluetoothDeviceListViewModel ViewModel;

        public DeviceInformationEx? SelectedDevice
        {
            get => (DeviceInformationEx?)this.GetValue(SelectedDeviceProperty);
            internal set => this.SetValue(SelectedDeviceProperty, value);
        }
        public static readonly DependencyProperty SelectedDeviceProperty = DependencyProperty.Register(
            nameof(SelectedDevice),
            typeof(DeviceInformationEx),
            typeof(BluetoothDeviceListView),
            new PropertyMetadata(null));

        public BluetoothDeviceListView()
        {
            this.InitializeComponent();
            ViewModel = new BluetoothDeviceListViewModel();
        }

        public Task DeviceScanStart() => ViewModel.DeviceScanStart();
        public void DeviceScanStop() => ViewModel.DeviceScanStop();

    }

}

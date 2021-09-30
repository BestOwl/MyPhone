using GoodTimeStudio.MyPhone.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        PhoneLineWatcher _LineWatcher;
        DeviceWatcher _DeviceWatcher;

        private object locker = new object();

        PhoneLineTransportDevice SelectedDevice;

        public TestPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_LineWatcher == null)
            {
                PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
                _LineWatcher = store.RequestLineWatcher();
                _LineWatcher.LineAdded += _watcher_LineAdded;
                _LineWatcher.LineRemoved += _watcher_LineRemoved;

                _LineWatcher.Start();
            }
            string str = PhoneLineTransportDevice.GetDeviceSelector(PhoneLineTransport.Bluetooth);
            if (_DeviceWatcher == null)
            {
                _DeviceWatcher = DeviceInformation.CreateWatcher(str);
                _DeviceWatcher.Added += _DeviceWatcher_Added;
                _DeviceWatcher.Removed += _DeviceWatcher_Removed;

                _DeviceWatcher.Start();
            }

            string str1 = BluetoothDevice.GetDeviceSelector();
            var watch = DeviceInformation.CreateWatcher(str1);
            watch.Added += _DeviceWatcher_Added;
            watch.Removed += _DeviceWatcher_Removed;
            watch.Start();
        }

        private async void _DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var de = ViewModel.PLTDevices.Where(d => d.Id == args.Id).FirstOrDefault();
                if (de != null)
                {
                    ViewModel.PLTDevices.Remove(de);
                }
            });
        }

        private async void _DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.PLTDevices.Add(args);
            });
        }

        private async void _watcher_LineAdded(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.PhoneLines.Add(args.LineId);
            });
        }

        private async void _watcher_LineRemoved(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Line removing: {0}", args.LineId);
#endif
                Guid id = args.LineId;
                ViewModel.PhoneLines.Remove(id);
            });
        }

        private async void Button_Click_Call(object sender, RoutedEventArgs e)
        {
            if (_PhoneLineList.SelectedItem == null)
            {
                return;
            }

            PhoneLine line = await PhoneLine.FromIdAsync((Guid)_PhoneLineList.SelectedItem);
            if (line == null)
            {
                return;
            }
            if (line.CanDial)
            {
                line.Dial(PhoneNumberBox.Text, PhoneNumberBox.Text);
            }
        }

        private async void Button_Click_ConnectDevice(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedDevice == null)
            {
                return;
            }
            ViewModel.IsWorking = true;
            
            if (await SelectedDevice.ConnectAsync())
            {
                ViewModel.ConnectionStatus = "Success";
            }
            else
            {
                ViewModel.ConnectionStatus = "Failed";
            }

            ViewModel.IsWorking = false;
        }

        private async void _PLTDList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.RegistrationStatus = string.Empty;

            PhoneLineTransportDevice device = PhoneLineTransportDevice.FromId(ViewModel.SelectedDevice.Id);
            if (device == null)
            {
                return;
            }

            SelectedDevice = device;
            var result = await device.RequestAccessAsync();
            if (result != DeviceAccessStatus.Allowed)
            {
                ViewModel.RegistrationStatus = result.ToString();
            }
            RefreashRegStatus();
        }

        private void Button_Click_RegisterApp(object sender, RoutedEventArgs e)
        {
            if (SelectedDevice != null)
            {
                SelectedDevice.RegisterApp();
                RefreashRegStatus();
            }
        }

        private void Button_Click_UnregisterApp(object sender, RoutedEventArgs e)
        {
            if (SelectedDevice != null)
            {
                SelectedDevice.UnregisterApp();
                RefreashRegStatus();
            }
        }

        private void RefreashRegStatus()
        {
            ViewModel.RegistrationStatus = SelectedDevice.IsRegistered().ToString();
        }

        private async void Button_Click_GetRfcommServices(object sender, RoutedEventArgs e)
        {
            BluetoothDevice bt = await BluetoothDevice.FromIdAsync(ViewModel.SelectedDevice.Id);
            RfcommDeviceServicesResult result = await bt.GetRfcommServicesAsync();
            foreach (var b in bt.SdpRecords)
            {
                Debug.WriteLine(Encoding.UTF8.GetString(b.ToArray()));
            }
            foreach (var service in result.Services)
            {
                Debug.WriteLine(service.ServiceId.AsString() + "    " + service.ConnectionHostName + "    " + service.ConnectionServiceName);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace GoodTimeStudio.MyPhone.Models
{
    public class BluetoothDeviceListViewModel : BindableBase
    {
        private DeviceWatcher _DeviceWatcher;

        public ObservableCollection<DeviceInformation> Devices;

        private DeviceInformation _SelectedDevice;
        public DeviceInformation SelectedDevice
        {
            get => _SelectedDevice;
            set => SetProperty(ref _SelectedDevice, value);
        }

        public BluetoothDeviceListViewModel()
        {
            Devices = new ObservableCollection<DeviceInformation>();
            _DeviceWatcher = DeviceInformation.CreateWatcher(PhoneLineTransportDevice.GetDeviceSelector(PhoneLineTransport.Bluetooth));
            _DeviceWatcher.Added += _DeviceWatcher_Added;
            _DeviceWatcher.Removed += _DeviceWatcher_Removed;
        }

        private void _DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var de = Devices.Where(d => d.Id == args.Id).FirstOrDefault();
            if (de != null)
            {
                Devices.Remove(de);
            }
        }

        private void _DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            Devices.Add(args);
        }

        public void DeviceScanStart()
        {
            _DeviceWatcher.Start();
        }

        public void DeviceScanStop()
        {
            _DeviceWatcher.Stop();
        }
    }
}

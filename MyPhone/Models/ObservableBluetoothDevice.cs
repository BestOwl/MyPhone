using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.Models
{
    public class ObservableBluetoothDevice : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public BluetoothDevice BluetoothDevice { get; private set; }

        public bool WasSecureConnectionUsedForPairing => BluetoothDevice.WasSecureConnectionUsedForPairing;
        public IReadOnlyList<IBuffer> SdpRecords => BluetoothDevice.SdpRecords;
        public IReadOnlyList<RfcommDeviceService> RfcommServices => BluetoothDevice.RfcommServices;
        public string Name => BluetoothDevice.Name;
        public HostName HostName => BluetoothDevice.HostName;
        public DeviceInformation DeviceInformation => BluetoothDevice.DeviceInformation;
        public string DeviceId => BluetoothDevice.DeviceId;
        public DeviceAccessInformation DeviceAccessInformation => BluetoothDevice.DeviceAccessInformation;
        public BluetoothConnectionStatus ConnectionStatus => BluetoothDevice.ConnectionStatus;
        public BluetoothClassOfDevice ClassOfDevice => BluetoothDevice.ClassOfDevice;
        public BluetoothDeviceId BluetoothDeviceId => BluetoothDevice.BluetoothDeviceId;
        public ulong BluetoothAddress => BluetoothDevice.BluetoothAddress;

        public string ClassOfDeviceDescription => $"{ClassOfDevice.MajorClass} - {ClassOfDevice.MinorClass}";

        public ObservableBluetoothDevice(BluetoothDevice bluetoothDevice)
        {
            BluetoothDevice = bluetoothDevice;
            BluetoothDevice.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
            BluetoothDevice.NameChanged += BluetoothDevice_NameChanged;
            BluetoothDevice.SdpRecordsChanged += BluetoothDevice_SdpRecordsChanged;
        }

        private void BluetoothDevice_SdpRecordsChanged(BluetoothDevice sender, object args)
        {
            BluetoothDevice = sender;
            OnPropertyChanged(nameof(SdpRecords));
        }

        private void BluetoothDevice_NameChanged(BluetoothDevice sender, object args)
        {
            BluetoothDevice = sender;
            OnPropertyChanged(nameof(Name));
        }

        private void BluetoothDevice_ConnectionStatusChanged(BluetoothDevice sender, object args)
        {
            BluetoothDevice = sender;
            OnPropertyChanged(nameof(ConnectionStatus));
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

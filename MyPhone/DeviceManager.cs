using GoodTimeStudio.MyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.System;

namespace GoodTimeStudio.MyPhone
{
    public class DeviceManager
    {
        public static DeviceInformation DeviceInfo;
        public static PhoneLineTransportDevice CallDevice;
        public static BluetoothDevice BthDevice;

        public static DeviceState State = DeviceState.Disconnected;

        public static async Task<bool> ConnectTo(DeviceInformation deviceInfo)
        {
            CallDevice = PhoneLineTransportDevice.FromId(deviceInfo.Id);
            if (CallDevice == null)
            {
                return false;
            }

            DeviceAccessStatus status = await CallDevice.RequestAccessAsync();
            if (status != DeviceAccessStatus.Allowed)
            {
                return false;
            }

            BthDevice = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
            DeviceInfo = deviceInfo;
            if (!CallDevice.IsRegistered())
            {
                CallDevice.RegisterApp();
            }
            State = DeviceState.Connected;

            return true;
        }
    }

    public enum DeviceState
    {
        Disconnected,
        LostConnection,
        Connected
    }
}

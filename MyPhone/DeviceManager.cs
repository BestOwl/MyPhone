using GoodTimeStudio.MyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone
{
    public class DeviceManager
    {
        public static DeviceInformation DeviceInfo;
        public static DeviceState State = DeviceState.Disconnected;

        public static async Task<bool> ConnectTo(DeviceInformation deviceInfo)
        {
            string result = await App.SendRequest("goodtimestudio.myphone.trayapp://connect/" + deviceInfo.Id);
            if (result.StartsWith("goodtimestudio.myphone://connect/"))
            {
                if (result.Substring(result.LastIndexOf('/') + 1) == "true")
                {
                    return true;
                }
            }
            return false;
        }
    }

    public enum DeviceState
    {
        Disconnected,
        LostConnection,
        Connected
    }
}

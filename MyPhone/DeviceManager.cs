using GoodTimeStudio.MyPhone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace GoodTimeStudio.MyPhone
{
    public class DeviceManager
    {
        public static DeviceInformation DeviceInfo;
        public static PhoneLine Line;
        public static Guid LineId;
        public static DeviceState State = DeviceState.Disconnected;

        private static PhoneLineWatcher LineWatcher;

        public static async Task Init()
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            if (settings.TryGetValue("deviceId", out object obj))
            {
                if (obj is string)
                {
                    string id = obj as string;
                    if (!string.IsNullOrEmpty(id))
                    {
                        DeviceInfo = await DeviceInformation.CreateFromIdAsync(id);
                        await EnsureInitPhoneLineWatcher();
                        LineWatcher.Start();
                    }
                }
                else
                {
                    settings.Remove("deviceId");
                }
            }
        }

        public static async Task<bool> ConnectTo(DeviceInformation deviceInfo)
        {
            if (!await EnsureInitPhoneLineWatcher())
            {
                return false;
            }

            string result = await App.SendRequest("goodtimestudio.myphone.trayapp://connect/" + deviceInfo.Id);
            if (result.StartsWith("goodtimestudio.myphone://connect/"))
            {
                if (result.Substring(result.LastIndexOf('/') + 1) == "true")
                {
                    DeviceInfo = deviceInfo;
                    var settings = ApplicationData.Current.LocalSettings.Values;
                    settings["deviceId"] = deviceInfo.Id;
                    LineWatcher.Start();
                    return true;
                }
            }
            return false;
        }

        public static void Call(string number)
        {
            if (Line !=null && Line.CanDial)
            {
                Line.Dial(number, number);
            }
        }

        public static async Task<bool> EnsureInitPhoneLineWatcher()
        {
            if (LineWatcher == null)
            {
                PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
                if (store == null)
                {
                    return false;
                }
                LineWatcher = store.RequestLineWatcher();
                LineWatcher.LineAdded += LineWatcher_LineAdded;
                LineWatcher.LineRemoved += LineWatcher_LineRemoved;
                LineWatcher.LineUpdated += LineWatcher_LineUpdated;
            }

            return true;
        }

        private static void LineWatcher_LineUpdated(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LineUpdated: " + args.LineId);
#endif
        }

        private static void LineWatcher_LineRemoved(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LineRemoved: " + args.LineId);
#endif
            if (Guid.Equals(args.LineId, LineId))
            {
                Line = null;
                LineId = Guid.Empty;
            }
        }

        private static async void LineWatcher_LineAdded(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LineAdded: " + args.LineId);
#endif
            PhoneLine line = await PhoneLine.FromIdAsync(args.LineId);
            if (line.TransportDeviceId == DeviceInfo.Id)
            {
                Line = line;
                LineId = args.LineId;
            }
        }
    }

    public enum DeviceState
    {
        Disconnected,
        LostConnection,
        Connected
    }
}

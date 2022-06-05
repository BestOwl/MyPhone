using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

using static GoodTimeStudio.MyPhone.Demo.HFP.ServiceLevelConnectionState;

namespace GoodTimeStudio.MyPhone.Demo
{
    /// <summary>
    /// Bluetooth HFPv1.7 HF implementation
    /// </summary>
    class HFP
    {
        // Hands Free Service Id
        public static readonly Guid HFP_Id = new Guid("0000111f-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// List HF supported features
        /// </summary>
        /// HFP 1.7 features
        /// Supported:
        ///     bit0: EC and/or NR function (echo canceling and noise reduction)
        ///     bit1: Three-way calling
        ///     bit2: CLI presentation capability
        ///     bit4: Remote volume control
        ///     bit5: Enhanced call status
        ///     bit6: Enhanced call control
        ///     bit7: Codec negotiation
        ///     bit8: HF Indicators
        ///     bit9: eSCO S4 (and T2) Settings Supported
        /// Not supported:
        ///     bit3: Voice recognition activation
        public const uint HF_Supported_Features = 1015; //0x11 1111 0111 

        /// <summary>
        /// List AG supported features
        /// </summary>
        /// HFP 1.7 features
        ///     bit0: Three-way calling
        ///     bit1: EC and/or NR function
        ///     bit2: Voice recognition function
        ///     bit3: In-band ring tone capability
        ///     bit4: Attach a number to a voice tag
        ///     bit5: Ability to reject a call
        ///     bit6: Enhanced call status
        ///     bit7: Enhanced call control
        ///     bit8: Extended Error Result Codes
        ///     bit9: Codec negotiation
        ///     bit10: HF Indicators
        ///     bit11: eSCO S4 (and T2) Settings Supported
        public static uint AG_Supported_Features;

        /// <summary>
        /// HF available codec (id)
        /// </summary>
        /// Id definition:
        /// 1. CVSD
        /// 2. mSBC
        public const string HF_Available_Codec = "1,2";

        /// <summary>
        /// HF supported indicators (id)
        /// </summary>
        /// https://www.bluetooth.com/specifications/assigned-numbers/hands-free-profile/
        /// Id definition:
        /// 1. Enhanced Safety (0: disabled / 1: enabled)
        /// 2. Battery Level (0~100)
        public const string HF_Supported_Indicators = "1,2";

        public enum AG_Indicators
        {
            Service,
            Call,
            CallSetup,
            BatteryCharged,
            Signal,
            Roam,
            CallHeld
        }

        public enum ServiceLevelConnectionState
        {
            Disconnected,
            Exchange_Supported_Features,            // AT+BRSF=
            Negotiate_Codecs,                       // AT+BAC=
            Retrieve_AG_Indicators,                 // AT+CIND=?
            Retrieve_AG_Indicators_Status,          // AT+CIND?
            Enable_AG_Indicators_Status_Update,     // AT+CMER=3,0,0,1
            Retrieve_Call_Hold_Capability,          // AT+CHLD=?
            List_HF_Indicators,                     // AT+BIND=1,2 
            Retrieve_Supported_HF_Indicators,       // AT+BIND=?
            Retrieve_Enable_HF_Indicators_Status,   // AT+BIND
            Established
        }

        private static ServiceLevelConnectionState _SLCState;
        public static ServiceLevelConnectionState SLCState
        {
            get => _SLCState;
            set
            {
                _SLCState = value;
                Console.WriteLine("Service level connection state: {0}", value);
            }
        }

        private static AutoResetEvent _PendingResult = new AutoResetEvent(false);

        public static BluetoothDevice BTDevice;

        public static RfcommDeviceService BTService;

        public static StreamSocket BTSocket;

        private static DataWriter _writer;

        private static DataReader _reader;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //Do().Wait();
            DoTest().Wait();
        }

        private static async Task<Dictionary<Guid, PhoneLine>> GetPhoneLinesAsync()
        {
            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();

            // Start the PhoneLineWatcher
            var watcher = store.RequestLineWatcher();
            var phoneLines = new List<PhoneLine>();
            var lineEnumerationCompletion = new TaskCompletionSource<bool>();
            watcher.LineAdded += async (o, args) =>
            {
                var line = await PhoneLine.FromIdAsync(args.LineId);
                phoneLines.Add(line);
            };
            watcher.Stopped += (o, args) => lineEnumerationCompletion.TrySetResult(false);
            watcher.EnumerationCompleted += (o, args) => lineEnumerationCompletion.TrySetResult(true);
            watcher.Start();

            // Wait for enumeration completion
            if (!await lineEnumerationCompletion.Task)
            {
                throw new Exception("Phone Line Enumeration failed");
            }

            watcher.Stop();

            Dictionary<Guid, PhoneLine> returnedLines = new Dictionary<Guid, PhoneLine>();

            foreach (PhoneLine phoneLine in phoneLines)
            {
                if (phoneLine != null && phoneLine.Transport == PhoneLineTransport.Cellular)
                {
                    returnedLines.Add(phoneLine.Id, phoneLine);
                    Console.WriteLine(phoneLine.Id);
                }
            }

            return returnedLines;
        }

        public static async Task DoTest()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(PhoneLineTransportDevice.GetDeviceSelector(PhoneLineTransport.Bluetooth));
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine(i + "#:    " + devices[i].Name + "    " + devices[i].Id);
            }

        select:
            Console.WriteLine("Please input device id to select");

            if (int.TryParse(Console.ReadLine(), out int s))
            {
                if (s < 0 || s > devices.Count)
                {
                    goto select;
                }
                Console.WriteLine("Selected: " + devices[s].Name + "    " + devices[s].Id);
            }
            else
            {
                goto select;
            }

            var PDevice = PhoneLineTransportDevice.FromId(devices[s].Id);
            var result = await PDevice.RequestAccessAsync();
            if (!PDevice.IsRegistered())
            {
                PDevice.RegisterApp();
            }
            var flag = await PDevice.ConnectAsync();
            if (flag)
            {
                Console.WriteLine("Connected");
            }
            else
            {
                Console.WriteLine("Could not connect to PhoneLineDevice");
            }
            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
            PhoneLine pl = await PhoneLine.FromIdAsync(await store.GetDefaultLineAsync());
            pl.DialWithOptions(new PhoneDialOptions { AudioEndpoint = PhoneAudioRoutingEndpoint.Bluetooth, Number = "18589846271" });

            Console.ReadLine();
            PDevice.UnregisterApp();
        }

        public static async Task Do()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine(i + "#:    " + devices[i].Name + "    " + devices[i].Id);
            }
        select:
            Console.WriteLine("Please input device id to select");

            if (int.TryParse(Console.ReadLine(), out int s))
            {
                if (s < 0 || s > devices.Count)
                {
                    goto select;
                }
                Console.WriteLine("Selected: " + devices[s].Name + "    " + devices[s].Id);
            }
            else
            {
                goto select;
            }

            bool flag = await HFPConnect(devices[s].Id);
            if (!flag)
            {
                goto select;
            }
            ReceiveLoop(_reader);
            Task t = GetPhoneLinesAsync();
            await SLCInit();

            QueryLoop(_writer).Wait();
        }

        public static async Task<bool> HFPConnect(string deviceId)
        {
            try
            {
                BTDevice = await BluetoothDevice.FromIdAsync(deviceId);
                var result = await BTDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(HFP_Id));
                if (result.Services.Count > 0)
                {
                    BTService = result.Services[0];
                    BTSocket = new StreamSocket();
                    await BTSocket.ConnectAsync(BTService.ConnectionHostName, BTService.ConnectionServiceName,
                        SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                    _writer = new DataWriter(BTSocket.OutputStream);
                    _reader = new DataReader(BTSocket.InputStream);

                    return true;
                }
                else
                {
                    Console.WriteLine("This device does not support HFP");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to HFP service of this device");
                Console.WriteLine(e);
                return false;
            }
        }

        public static async Task SLCInit()
        {
            SLCState = Exchange_Supported_Features;
            await PushCommand("AT+BRSF=" + HF_Supported_Features);

            if ((AG_Supported_Features >> 9 & 0x1) == 1) // Check if the device support Codec Negotiation
            {
                Console.WriteLine("This device support Codec Negotiation");
                SLCState = Negotiate_Codecs;
                await PushCommand("AT+BAC=" + HF_Available_Codec);
            }
            else
            {
                Console.WriteLine("This device does not support Codec Negotiation");
            }
            SLCState = Retrieve_AG_Indicators;
            await PushCommand("AT+CIND=?");
            SLCState = Retrieve_AG_Indicators_Status;
            await PushCommand("AT+CIND?");
            SLCState = Enable_AG_Indicators_Status_Update;
            await PushCommand("AT+CMER=3,0,0,1");
            if ((AG_Supported_Features & 0x1) == 1)
            {
                Console.WriteLine("This device support three-way calling");
                SLCState = Retrieve_Call_Hold_Capability;
                await PushCommand("AT+CHLD=?");
            }
            else
            {
                Console.WriteLine("This device does not support three-way calling");
            }
            if ((AG_Supported_Features >> 10 & 0x1) == 1)
            {
                Console.WriteLine("This device support HF indicators");
                SLCState = List_HF_Indicators;
                await PushCommand("AT+BIND=" + HF_Supported_Indicators);
                SLCState = Retrieve_Supported_HF_Indicators;
                await PushCommand("AT+BIND=?");
                SLCState = Retrieve_Enable_HF_Indicators_Status;
                await PushCommand("AT+BIND?");
            }
            else
            {
                Console.WriteLine("This device does not support HF indicators");
            }

            SLCState = Established;


            var adapter = await BluetoothAdapter.GetDefaultAsync();
            var radio = await adapter.GetRadioAsync();

        }

        public static async Task PushCommand(string command)
        {
            _writer.WriteString(command + "\r\n");
            await _writer.StoreAsync();
            Console.WriteLine("Sent: {0}", command);
            _PendingResult.WaitOne();
            await Task.Delay(50);
        }

        public static Task QueryLoop(DataWriter writer)
        {
            Task t = Task.Run(async () =>
            {

                while (true)
                {
                    string msg = Console.ReadLine();
                    writer.WriteString(msg);
                    writer.WriteString("\r\n");
                    await writer.StoreAsync();
                    Console.WriteLine("Sent: {0}", msg);
                    await Task.Delay(50);
                }
            }
            );
            return t;
        }

        public static void ReceiveLoop(DataReader reader)
        {
            Task t = Task.Run(async () =>
            {
                StringBuilder receiveBuffer = new StringBuilder();
                while (true)
                {
                    uint buf;
                    buf = await reader.LoadAsync(1);
                    if (reader.UnconsumedBufferLength > 0)
                    {
                        string s = reader.ReadString(1);
                        if (s.Equals("\n") || s.Equals("\r"))
                        {
                            try
                            {
                                OnMessageReceived(receiveBuffer.ToString());
                                receiveBuffer.Clear();
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine(exc.Message);
                            }
                        }
                        else
                        {
                            receiveBuffer.Append(s);
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0));
                    }
                }
            });
        }

        public static async Task OnMessageReceived(string message)
        {
            // Skip empty message
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Console.WriteLine("Message Received:" + message);

            if (message.Equals("OK") || message.Equals("ERROR"))
            {
                _PendingResult.Set();
                Console.WriteLine();
            }
            else if (message.StartsWith("+BRSF:"))
            {
                if (uint.TryParse(message.Substring(6), out uint i))
                {
                    AG_Supported_Features = i;
                }
            }
            else if (message.StartsWith("+BCS:"))
            {
                if (uint.TryParse(message.Substring(5), out uint i))
                {
                    await PushCommand("AT+BCS=" + i);
                    await DoTest();
                }
            }
        }
    }
}

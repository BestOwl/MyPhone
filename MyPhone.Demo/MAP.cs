using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace MyPhone.Demo
{
    class MAP
    {

        public static readonly Guid MAP_Id = new Guid("00001132-0000-1000-8000-00805f9b34fb");
        public static readonly Guid MAP_MNS_Id = new Guid("00001133-0000-1000-8000-00805f9b34fb");

        //bb582b40-420c-11db-b0de-0800200c9a66
        public static readonly byte[] MAS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

        public static readonly byte[] MNS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x41, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

        public static BluetoothDevice BTDevice;

        public static RfcommDeviceService BTService;

        public static StreamSocket BTSocket;

        public static StreamSocketListener BT_MNS_Server;

        public static RfcommServiceProvider BT_MNS_Provider;

        private static DataWriter _writer;

        private static DataReader _reader;

        public static void Main(string[] args)
        {
            Do().Wait();
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

            bool flag = await SocketConnect(devices[s].Id, MAP_Id);
            if (!flag)
            {
                goto select;
            }

            await MAP_Init();

            await MNS_Init();

            Console.WriteLine("Enter any key to exit...");
            Console.ReadLine();
            BT_MNS_Provider.StopAdvertising();
        }

        public static async Task<bool> SocketConnect(string deviceId, Guid guid)
        {
            try
            {
                BTDevice = await BluetoothDevice.FromIdAsync(deviceId);

                var result = await BTDevice.GetRfcommServicesAsync();
                Console.WriteLine();
                Console.WriteLine("Available RFComm services for this devices: ");
                foreach (var ser in result.Services)
                {
                    Console.WriteLine(ser.ConnectionServiceName);
                }
                Console.WriteLine("Press any key to proceed");
                Console.WriteLine();
                Console.ReadKey();

                result = await BTDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(guid));
                if (result.Services.Count > 0)
                {
                    BTService = result.Services[0];

                    //var dict = await BTService.GetSdpRawAttributesAsync();
                    //Console.WriteLine("Raw SDP for MAP");
                    //foreach (var kv in dict)
                    //{
                    //    Console.WriteLine(kv.Key);
                    //    Console.WriteLine(BitConverter.ToString(kv.Value.ToArray()));
                    //    Console.WriteLine("============");
                    //}
                    //Console.WriteLine();

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

        public async static Task MAP_Init()
        {
            _writer.WriteByte(0x80);
            _writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16BE;
            _reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16BE;

            DataWriter appWriter = new DataWriter();
            appWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16BE;
            appWriter.WriteByte(0x10);                  //version code 1.0
            appWriter.WriteByte(0);                       //flag
            appWriter.WriteUInt16(0xFFFF);          // max packet length
            
            appWriter.WriteByte(0x46);                 // Header: target
            appWriter.WriteUInt16(19);                // length
            appWriter.WriteBytes(MAS_UUID);    

            IBuffer buffer = appWriter.DetachBuffer();

            _writer.WriteUInt16((ushort)(buffer.Length + 3));
            _writer.WriteBuffer(buffer);
            await _writer.StoreAsync();

            uint loaded = await _reader.LoadAsync(1);
            if (loaded <= 0)
            {
                Console.WriteLine("Failed.");
                return;
            }

            byte responseOpCode = _reader.ReadByte();
            Console.WriteLine("Response OpCode: " + responseOpCode.ToString());
            if (responseOpCode == 160)
            {
                Console.WriteLine("Connection established successfully!");
            }

            loaded = await _reader.LoadAsync(2);
            if (loaded <= 0)
            {
                Console.WriteLine("Failed.");
                return;
            }
            ushort responseLen = _reader.ReadUInt16();
            Console.WriteLine("Response length: " + responseLen);

            loaded = await _reader.LoadAsync((uint)(responseLen - 3));
            if (loaded < 1)
            {
                Console.WriteLine("Failed.");
                return;
            }
            byte[] bytesBuf = new byte[responseLen - 3];
            _reader.ReadBytes(bytesBuf);
            Console.WriteLine(BitConverter.ToString(bytesBuf));
        }

        public async static Task MNS_Init()
        {
            BT_MNS_Server = new StreamSocketListener();
            BT_MNS_Provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(MAP_MNS_Id));

            //var rawSDP = await BTService.GetSdpRawAttributesAsync();
            //foreach (var kv in rawSDP)
            //{
            //    BT_MNS_Provider.SdpRawAttributes.Add(kv.Key, kv.Value);
            //}

            //Console.WriteLine();
            //foreach (var kv in BT_MNS_Provider.SdpRawAttributes)
            //{
            //    Console.WriteLine(kv.Key);
            //}
            //Console.WriteLine();

            //DataWriter writer = new DataWriter();
            //// First write the attribute type
            //writer.WriteByte(0x0A);
            //// Then write the data
            //writer.WriteUInt32(200);
            //IBuffer data = writer.DetachBuffer();
            //BT_MNS_Provider.SdpRawAttributes.Add(0x0300, data);

            BT_MNS_Server.ConnectionReceived += MNS_Listener_ConnectionReceived;
            await BT_MNS_Server.BindServiceNameAsync(BT_MNS_Provider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
            BT_MNS_Provider.StartAdvertising(BT_MNS_Server, true);
            Console.WriteLine("MNS: Start advertising MNS service");

            Console.WriteLine("Wait for 1 seconds");
            await Task.Delay(1000);

            DataWriter appWriter = new DataWriter();

            //appWriter.WriteByte(0xCB);         // Header: Connection Id
            //appWriter.WriteUInt32(1);           // Connection Id //TODO: read from MAS

            string type = "x-bt/MAP-NotificationRegistration";
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);

            appWriter.WriteByte(0x42);         // Header: Type
            appWriter.WriteUInt16((ushort)(typeBytes.Length + 1 +3));
            appWriter.WriteBytes(typeBytes);
            appWriter.WriteByte(0);          // \0 terminating

            appWriter.WriteByte(0x4C);         // Header: App. para
            appWriter.WriteUInt16(6);
            appWriter.WriteByte(0x0E);         // App. para: NotificationStatus
            appWriter.WriteByte(1);
            appWriter.WriteByte(1);
            
            appWriter.WriteByte(0x48);         // Header: Body
            appWriter.WriteInt16(4);
            appWriter.WriteByte(0x30);         // Filler-Byte 0x30

            IBuffer buffer = appWriter.DetachBuffer();
            Console.WriteLine("MNS: Sending");
            Console.WriteLine(BitConverter.ToString(buffer.ToArray()));

            _writer.WriteByte(0x82);               // Put Op
            _writer.WriteUInt16((ushort)(buffer.Length + 3));
            _writer.WriteBuffer(buffer);
            await _writer.StoreAsync();

            uint loaded = await _reader.LoadAsync(1);
            if (loaded < 1)
            {
                Console.WriteLine("MNS: Failed to call SetNotificationRegistration");
                return;
            }
            Console.WriteLine(_reader.ReadByte().ToString());
        }

        private async static void MNS_Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("Connection received");
            Console.WriteLine("Stoping Advertising");
            BT_MNS_Provider.StopAdvertising();
            StreamSocket socket = args.Socket;
            DataReader reader = new DataReader(socket.InputStream);
            DataWriter writer = new DataWriter(socket.OutputStream);

            uint loaded = await reader.LoadAsync(3);
            if (loaded < 1)
            {
                Console.WriteLine("MNS: Failed.");
                return;
            }
            Console.WriteLine("MNS: Op Code: " + reader.ReadByte().ToString());
            ushort len = reader.ReadUInt16();
            await reader.LoadAsync((uint)(len - 3));

            byte[] bytesBuf = new byte[len - 3];
            reader.ReadBytes(bytesBuf);
            Console.WriteLine("MNS: " + BitConverter.ToString(bytesBuf));

            DataWriter appWriter = new DataWriter();
            appWriter.WriteBytes(new byte[] { 0x10, 0x00, 0xFF, 0xFE, 0xCB, 0x00, 0x00, 0x00, 0x01, 0x4A, 0x00, 0x13 });
            appWriter.WriteBytes(MNS_UUID);

            IBuffer buffer = appWriter.DetachBuffer();

            writer.WriteByte(0xA0);
            writer.WriteUInt16((ushort)(buffer.Length + 3));
            writer.WriteBuffer(buffer);
            await writer.StoreAsync();

            while (true)
            {
                loaded = await reader.LoadAsync(3);
                if (loaded < 1)
                {
                    Console.WriteLine("MNS: Failed.");
                    return;
                }
                Console.WriteLine("MNS: Op Code: " + reader.ReadByte().ToString());
                len = reader.ReadUInt16();
                await reader.LoadAsync((uint)(len - 3));

                bytesBuf = new byte[len - 3];
                reader.ReadBytes(bytesBuf);
                Console.WriteLine("MNS: " + BitConverter.ToString(bytesBuf));

                writer.WriteByte(0xA0);
                writer.WriteUInt16(3);
                await writer.StoreAsync();
            }
            
        }
    }
}

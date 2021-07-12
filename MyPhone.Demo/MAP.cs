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
using MyPhone.OBEX;

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

            if (await MAP_Init())
            {
                await MNS_Init();
            }

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
                Console.WriteLine("Connecting...");

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

        public async static Task<bool> MAP_Init()
        {
            MASConnectPacket mASInitPacket = new MASConnectPacket();
            _writer.WriteBuffer(mASInitPacket.ToBuffer());
            await _writer.StoreAsync();

            OBEXPacket packet = await OBEXPacket.ReadFromStream(_reader);
            if (packet == null || packet.Opcode != Opcode.Success)
            {
                Console.WriteLine("Failed");
                return false;
            }

            return true;
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

            OBEXPacket packet = new OBEXPacket(new Int32ValueHeader(HeaderId.ConnectionId, 1), //TODO: read from MAS
                new StringValueHeader(HeaderId.Type, "x-bt/MAP-NotificationRegistration"),
                new AppParamHeader(AppParamTagId.NotificationStatus, 1),
                new BytesHeader(HeaderId.Body, 0x30)); // Filler-Byte 0x30
            packet.Opcode = Opcode.PutAlter;

            Console.WriteLine("MNS: Sending");
            IBuffer buf = packet.ToBuffer();
            Console.WriteLine(BitConverter.ToString(buf.ToArray()));

            _writer.WriteBuffer(buf);
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
                byte opCode = reader.ReadByte();
                Console.WriteLine("MNS: Op Code: " + opCode.ToString());

                if (opCode == 0x02 || opCode == 0x82)
                {
                    len = reader.ReadUInt16();
                    await reader.LoadAsync((uint)(len - 3));

                    Console.WriteLine("MNS: Received SendEvent");
                    Console.WriteLine("****************************");

                    // TODO: DO NOT DO THIS (DO NOT SKIP HI)
                    reader.ReadByte();  // skip HI: 0xCB

                    uint connectionId = reader.ReadUInt32();
                    Console.WriteLine("Connection Id: " + connectionId);

                    reader.ReadByte(); // skip HI: 0x42
                    ushort ulen = reader.ReadUInt16();
                    byte[] buf = new byte[ulen];
                    reader.ReadBytes(buf);
                    string type = Encoding.ASCII.GetString(buf);
                    Console.WriteLine("Type: " + type);

                    reader.ReadByte(); // skip HI: 0x4C (app. para.)
                    reader.ReadByte(); // skip app. para. len
                    byte _MAS_InstanceId = reader.ReadByte();
                    Console.WriteLine("MASInstanceID: " + _MAS_InstanceId);

                    reader.ReadByte(); // skip HI: 0x48 (Body)
                    ulen = (ushort)(reader.ReadUInt16() - 3);
                    buf = new byte[ulen];
                    reader.ReadBytes(buf);
                    Console.WriteLine("Body: ");
                    Console.WriteLine(Encoding.UTF8.GetString(buf));

                    //bytesBuf = new byte[len - 3];
                    //reader.ReadBytes(bytesBuf);
                    //Console.WriteLine("MNS: " + BitConverter.ToString(bytesBuf));

                    writer.WriteByte(0xA0);
                    writer.WriteUInt16(3);
                    await writer.StoreAsync();
                }
                else
                {
                    _writer.WriteByte(0xC6);
                    _writer.WriteUInt16(3);
                    await _writer.StoreAsync();
                }
                
            }
            
        }

    }
}

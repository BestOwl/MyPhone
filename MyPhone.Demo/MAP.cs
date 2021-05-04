using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace MyPhone.Demo
{
    class MAP
    {

        public static readonly Guid MAP_Id = new Guid("00001132-0000-1000-8000-00805f9b34fb");

        //bb582b40-420c-11db-b0de-0800200c9a66
        public static readonly byte[] MAS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

        public static readonly Guid MNS_Id = new Guid("bb582b41-420c-11db-b0de-0800200c9a66");

        public static BluetoothDevice BTDevice;

        public static RfcommDeviceService BTService;

        public static StreamSocket BTSocket;

        private static DataWriter _writer;

        private static DataReader _reader;

        public static void Main(string[] args)
        {
            Console.WriteLine(BitConverter.ToString(MAS_Id.ToByteArray()));
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
            Console.WriteLine(responseOpCode);

            loaded = await _reader.LoadAsync(2);
            if (loaded <= 0)
            {
                return;
            }
            ushort responseLen = _reader.ReadUInt16();
            Console.WriteLine("Response length: " + responseLen);

            //loaded = await  _reader.LoadAsync(1);
            //if (loaded < 1)
            //{
            //    return;
            //}
            //Console.WriteLine("Version code: " + _reader.ReadByte());

            loaded = await _reader.LoadAsync((uint)(responseLen - 3));
            if (loaded < 1)
            {
                return;
            }
            byte[] bytesBuf = new byte[responseLen - 3];
            _reader.ReadBytes(bytesBuf);
            Console.WriteLine(BitConverter.ToString(bytesBuf));

            //Console.WriteLine(responseLen);
            //short version = _reader.ReadByte();
            //Console.WriteLine(version);
            //short flag = _reader.ReadByte();
            //Console.WriteLine(flag);
            //short maxLen = _reader.ReadInt16();
            //Console.WriteLine(maxLen);
        }
    }
}

using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace MyPhone.Demo
{
    class MapClient
    {
        public static readonly Guid MAP_Id = new Guid("00001132-0000-1000-8000-00805f9b34fb");
        public static readonly Guid MAP_MNS_Id = new Guid("00001133-0000-1000-8000-00805f9b34fb");


        // PC hardware id for MAS
        // BTHENUM\{00001132-0000-1000-8000-00805f9b34fb}_VID&0001004c_PID&730d
        // BTHENUM\{00001132-0000-1000-8000-00805f9b34fb}_LOCALMFG&0002
        // 


        // MAP_Specification
        // OBEX Target header UUID
        //values used for the OBEX
        //services MAS and MNS
        //Target header UUID value
        //MAS bb582b40-420c-11db-b0de-0800200c9a66
        //MNS bb582b41-420c-11db-b0de-0800200c9a66

        //bb582b40-420c-11db-b0de-0800200c9a66
        public static readonly byte[] MAS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

        public static readonly byte[] MNS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x41, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };


        public StreamSocketListener BT_MNS_Server;

        public RfcommServiceProvider BT_MNS_Provider;


        // The Id of the Service Name SDP attribute
        public const UInt16 SdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        //    -  the Attribute Type size in the least significant 3 bits,
        //    -  the SDP Attribute Type value in the most significant 5 bits.
        public const byte SdpServiceNameAttributeType = (4 << 3) | 5;

        public BluetoothDevice BTDevice;

        public RfcommDeviceService BTService;

        public StreamSocket BTSocket;

        private DataWriter _writer;

        private DataReader _reader;


        public async Task<bool> MapClientConnect(string deviceId)
        {
            try
            {
                BTDevice = await BluetoothDevice.FromIdAsync(deviceId);

                Console.WriteLine("Connecting...");

                // This should return a list of uncached Bluetooth services 
                // (so if the server was not active when paired, it will still be detected by this call
                RfcommDeviceServicesResult result = await BTDevice.GetRfcommServicesForIdAsync(
                    RfcommServiceId.FromUuid(MAP_Id),
                    BluetoothCacheMode.Uncached);

                if (result.Services.Count > 0)
                {
                    BTService = result.Services[0];

                    // Do various checks of the SDP record to make sure you are talking to a device that actually supports the Bluetooth Rfcomm Chat Service
                    var attributes = await BTService.GetSdpRawAttributesAsync();
                    if (!attributes.ContainsKey(SdpServiceNameAttributeId))
                    {
                        Console.WriteLine("The Chat service is not advertising the Service Name attribute (attribute id=0x100). " +
                            "Please verify that you are running the BluetoothRfcommChat server.");
                        return false;
                    }
                    var attributeReader = DataReader.FromBuffer(attributes[SdpServiceNameAttributeId]);
                    var attributeType = attributeReader.ReadByte();
                    if (attributeType != SdpServiceNameAttributeType)
                    {
                        Console.WriteLine("The Chat service is using an unexpected format for the Service Name attribute. " +
                            "Please verify that you are running the BluetoothRfcommChat server.");
                        return false;
                    }
                    var serviceNameLength = attributeReader.ReadByte();

                    // The Service Name attribute requires UTF-8 encoding.
                    attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                    lock (this)
                    {
                        BTSocket = new StreamSocket();
                    }
                    try
                    {
                        await BTSocket.ConnectAsync(BTService.ConnectionHostName, BTService.ConnectionServiceName
                            //SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
                            );                            
                        _writer = new DataWriter(BTSocket.OutputStream);
                        _reader = new DataReader(BTSocket.InputStream);
                        //ReceiveStringLoop(_reader);
                    }
                    catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
                    {
                        Console.WriteLine("Please verify that you are running the BluetoothRfcommChat server.");
                        return false;
                    }
                    catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
                    {
                        Console.WriteLine("Please verify that there is no other RFCOMM connection to the same device.");
                        return false;
                    }

                    Console.WriteLine("Local connected");
                    return true;
                }
                else
                {
                    Console.WriteLine("This device does not support MAP service");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to MAP service of this device");
                Console.WriteLine(e);
                return false;
            }
        }

        public List<string> MessageList = new List<string>();

        private async void ReceiveStringLoop(DataReader reader)
        {
            try
            {
                uint size = await reader.LoadAsync(sizeof(uint));
                if (size < sizeof(uint))
                {
                    Disconnect("Remote device terminated connection - make sure only one instance of server is running on remote device");
                    return;
                }

                uint stringLength = reader.ReadUInt32();
                uint actualStringLength = await reader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
                    return;
                }

                
                MessageList.Add("Received: " + reader.ReadString(stringLength));

                ReceiveStringLoop(reader);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (BTSocket == null)
                    {
                        // Do not print anything here -  the user closed the socket.
                        if ((uint)ex.HResult == 0x80072745)
                            Console.WriteLine("Disconnect triggered by remote device");
                        else if ((uint)ex.HResult == 0x800703E3)
                            Console.WriteLine("The I/O operation has been aborted because of either a thread exit or an application request.");
                    }
                    else
                    {
                        Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }

        private void Disconnect(string disconnectReason)
        {
            if (_writer != null)
            {
                _writer.DetachStream();
                _writer = null;
            }


            if (BTService != null)
            {
                BTService.Dispose();
                BTService = null;
            }
            lock (this)
            {
                if (BTSocket != null)
                {
                    BTSocket.Dispose();
                    BTSocket = null;
                }
            }

            Console.WriteLine(disconnectReason);           
            
        }

        
        public async Task<bool> MapRemoteConnect()
        {
            if (_writer == null)
                return false;

            OBEXConnectPacket mASConnectPacket = new OBEXConnectPacket();

            Console.WriteLine("Sending Connect Request packet:");
            bool status = await RunObexRequest(mASConnectPacket);            
            return status;
        }


        public async Task<bool> RemoteNotificationRegister()
        {
            OBEXPacket packet = new OBEXPacket(
                //HeaderConnectionId, //TODO: read from MAS
                new Int32ValueHeader(HeaderId.ConnectionId, 1),
                new StringValueHeader(HeaderId.Type, "x-bt/MAP-NotificationRegistration"),
                new AppParamHeader(new AppParameter(AppParamTagId.NotificationStatus, 1)),
                new BytesHeader(HeaderId.Body, 0x30),
                new BytesHeader(HeaderId.Target, MAS_UUID)
                );
            packet.Opcode = Opcode.PutAlter;

            Console.WriteLine("Sending Connect Request packet:");
            bool status = await RunObexRequest(packet);

            return status;
        }

        private async Task BuildPcMns()
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
            await BT_MNS_Server.BindServiceNameAsync(BT_MNS_Provider.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionWithAuthentication
             );
            BT_MNS_Provider.StartAdvertising(BT_MNS_Server, true);
            Console.WriteLine("MNS: Start advertising MNS service");

            Console.WriteLine("Wait for 1 seconds");
            await Task.Delay(1000);
        }

        private async void MNS_Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("Connection received");
            Console.WriteLine("Stoping Advertising");
            BT_MNS_Provider.StopAdvertising();
            StreamSocket socket = args.Socket;
            DataReader reader = new DataReader(socket.InputStream);
            DataWriter writer = new DataWriter(socket.OutputStream);

            OBEXPacket packet = await OBEXPacket.ReadFromStream(reader, new OBEXConnectPacket());
            if (packet == null)
            {
                Console.WriteLine("MNS: Failed.");
                return;
            }

            if (packet.Opcode != Opcode.Connect)
            {
                Console.WriteLine("Not support operation code: " + packet.Opcode);
                Console.WriteLine("MSE should send Connect request first");
                return;
            }

            Console.WriteLine("Opcode: " + packet.Opcode);

            packet.Opcode = Opcode.Success;
            writer.WriteBuffer(packet.ToBuffer());
            await writer.StoreAsync();

            while (true)
            {
                packet = await OBEXPacket.ReadFromStream(reader);
                if (packet != null)
                {
                    Console.WriteLine("Opcode: " + packet.Opcode);

                    if (packet.Opcode == Opcode.Put || packet.Opcode == Opcode.PutAlter)
                    {
                        writer.WriteByte(0xA0); // Success
                        writer.WriteUInt16(3);
                        await writer.StoreAsync();
                    }
                    else
                    {
                        _writer.WriteByte(0xC6); // Not Acceptable
                        _writer.WriteUInt16(3);
                        await _writer.StoreAsync();
                    }
                }

            }            

        }


        private async Task<bool> RunObexRequest(OBEXPacket req)
        {
            Console.WriteLine("Sending request packet:");
            var buf = req.ToBuffer();
            Console.WriteLine(BitConverter.ToString(buf.ToArray()));
            Console.WriteLine("Opcode: " + req.Opcode);
            _writer.WriteBuffer(buf);

            try
            {
                await _writer.StoreAsync();
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072745)
            {
                Console.WriteLine("Remote side disconnect: " + ex.HResult.ToString() + " - " + ex.Message);
                return false;
            }


            Console.WriteLine("Req packet sent. Waiting for reply...");

            try
            {
                OBEXPacket packet = await OBEXPacket.ReadFromStream(_reader);

                if (packet != null)
                {
                    var bytes = packet.ToBuffer().ToArray();
                    Console.WriteLine("Reply packet:");
                    Console.WriteLine(BitConverter.ToString(bytes));
                    Console.WriteLine($"Opcode: {packet.Opcode}");

                    foreach (var header in packet.Headers)
                    {
                        Console.WriteLine($"{header.HeaderId}: {header.ToBytes()}");
                    }
                } 

                if (packet == null || (packet.Opcode != Opcode.Success && packet.Opcode != Opcode.SuccessAlt && packet.Opcode != Opcode.Continue && packet.Opcode != Opcode.ContinueAlt))
                {
                    Console.WriteLine("Remote request failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Remote request exception: " + ex.Message);
                return false;
            }

            Console.WriteLine("Request returned success");
            return true;
        }
    }
}

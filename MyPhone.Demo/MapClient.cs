using MyPhone.OBEX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace MyPhone.Demo
{
    class MapClient
    {
        public static readonly Guid MAP_Id = new Guid("00001132-0000-1000-8000-00805f9b34fb");
        public static readonly Guid MAP_MNS_Id = new Guid("00001133-0000-1000-8000-00805f9b34fb");
        public static readonly Guid PHONE_BOOK_ACCESS_ID = new Guid("0000112f-0000-1000-8000-00805f9b34fb");

        // PC hardware id for MAS
        // BTHENUM\{00001132-0000-1000-8000-00805f9b34fb}_VID&0001004c_PID&730d
        // BTHENUM\{00001132-0000-1000-8000-00805f9b34fb}_LOCALMFG&0002



        // MAP_Specification
        // OBEX Target header UUID
        //values used for the OBEX
        //services MAS and MNS
        //Target header UUID value
        //MAS bb582b40-420c-11db-b0de-0800200c9a66
        //MNS bb582b41-420c-11db-b0de-0800200c9a66
        
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
        public MasClient MasClient;
        public MnsServer MnsServer;

        public ConcurrentQueue<string> RequestQueue = new ConcurrentQueue<string>();


        public async Task<bool> ClientBTConnect(string deviceId)
        {
            try
            {
                BTDevice = await BluetoothDevice.FromIdAsync(deviceId);

                Console.WriteLine("Connecting to phone BT client...");

                var svcresults = await BTDevice.GetRfcommServicesAsync();
                if (svcresults.Services.Count > 0)
                {
                    foreach (var svc in svcresults.Services)
                    {
                        Console.WriteLine($"Host: {svc.ConnectionHostName}, ServiceId: {svc.ServiceId}, Service: {svc.ConnectionServiceName}, Access: {svc.DeviceAccessInformation.CurrentStatus} ");
                    }
                }

                // This should return a list of uncached Bluetooth services 
                // (so if the server was not active when paired, it will still be detected by this call
                RfcommDeviceServicesResult result = await BTDevice.GetRfcommServicesForIdAsync(
                    RfcommServiceId.FromUuid(MAP_Id)
                    , BluetoothCacheMode.Uncached
                );

                if (result.Services.Count > 0)
                {
                    BTService = result.Services[0];


                    var accessStatus = await BTService.RequestAccessAsync();
                    if (accessStatus == DeviceAccessStatus.Allowed)
                    {

                        Console.WriteLine("SDP Attributes");

                        // Do various checks of the SDP record to make sure you are talking to a device that actually supports the Bluetooth Rfcomm Chat Service
                        var attributes = await BTService.GetSdpRawAttributesAsync();
                        foreach (var item in attributes)
                        {
                            Console.WriteLine($"{item.Key}: {item.Value}");
                        }
                        if (!attributes.ContainsKey(SdpServiceNameAttributeId))
                        {
                            Console.WriteLine("The Phone BT service is not advertising the Service Name attribute (attribute id=0x100). " +
                                "Please verify that you are running the BluetoothRfcommChat server.");
                            return false;
                        }
                        var attributeReader = DataReader.FromBuffer(attributes[SdpServiceNameAttributeId]);
                        var attributeType = attributeReader.ReadByte();
                        if (attributeType != SdpServiceNameAttributeType)
                        {
                            Console.WriteLine("The Phone BT service is using an unexpected format for the Service Name attribute. " +
                                "Please verify that you are running the BluetoothRfcommChat server.");
                            return false;
                        }
                        var serviceNameLength = attributeReader.ReadByte();

                        // The Service Name attribute requires UTF-8 encoding.
                        attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                        var serviceName = attributeReader.ReadString(serviceNameLength);
                        Console.WriteLine($"Service: {serviceName},  Device: {BTDevice.Name}");

                    }

                    lock (this)
                    {
                        BTSocket = new StreamSocket();
                    }
                    try
                    {
                        await BTSocket.ConnectAsync(BTService.ConnectionHostName, BTService.ConnectionServiceName
                            , SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
                            );

                        MasClient = new MasClient(BTSocket.InputStream, BTSocket.OutputStream);
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

        public void Disconnect(string disconnectReason)
        {
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

        public async Task BuildPcMns()
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

        private void MNS_Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("Connection received");
            Console.WriteLine("Stoping Advertising");
            BT_MNS_Provider.StopAdvertising();
            StreamSocket socket = args.Socket;

            MnsServer = new MnsServer(socket.InputStream, socket.OutputStream);
            MnsServer.MessageReceived += MnsServer_MessageReceived;
            MnsServer.StartServer();
        }

        private void MnsServer_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            RequestQueue.Enqueue(e.MessageHandle);
        }
    }
}

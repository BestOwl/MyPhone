using MyPhone.OBEX;
using System;
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

        public Int32ValueHeader ConnectionHeader { get; set; } = new Int32ValueHeader(HeaderId.ConnectionId, 0);


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

        public void Disconnect(string disconnectReason)
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


        public async Task<bool> MasObexConnect()
        {
            if (_writer == null)
                return false;


            try
            {
                OBEXConnectPacket packet = new OBEXConnectPacket();
                var buf = packet.ToBuffer();

                Console.WriteLine("Sending OBEX Connection request to Phone MAP server:");
                Console.WriteLine(BitConverter.ToString(buf.ToArray()));
                Console.WriteLine("Opcode: " + packet.Opcode);
                _writer.WriteBuffer(buf);
                await _writer.StoreAsync();

                Console.WriteLine("Waiting reply packet...");
                OBEXPacket retPacket = await OBEXPacket.ReadFromStream(_reader, packet);

                if (retPacket != null)
                {
                    var bytes = retPacket.ToBuffer().ToArray();
                    Console.WriteLine("Reply packet:");
                    Console.WriteLine(BitConverter.ToString(bytes));
                    Console.WriteLine($"ResponseCode: {retPacket.Opcode}");

                    ConnectionHeader.Value = ((Int32ValueHeader)retPacket.Headers[HeaderId.ConnectionId]).Value;

                    printh(retPacket.Headers.Values);
                }

                if (retPacket == null ||
                    (retPacket.Opcode != Opcode.Success
                    && retPacket.Opcode != Opcode.SuccessAlt
                    && retPacket.Opcode != Opcode.Continue
                    && retPacket.Opcode != Opcode.ContinueAlt
                ))
                {
                    Console.WriteLine("Remote request failed.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }



            return true;
        }


        public async Task<bool> ObexConnect()
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.Connect
                , new AsciiStringValueHeader(HeaderId.Type, "x-obex/folder-listing")
            //, new AppParamHeader(new AppParameter(AppParamTagId.MaxListCount, 100))
            );

            Console.WriteLine("sending GetFolderList request");

            OBEXPacket resp = await RunObexRequest(packet);
            return resp.Opcode.Equals(Opcode.Success) || resp.Opcode.Equals(Opcode.SuccessAlt);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxListCount"></param>
        /// <param name="folderName"></param>
        /// <returns>message handle list</returns>
        /// TODO: return Messages-Listing objects
        public async Task<List<string>> GetMessageListing(ushort maxListCount, string folderName = "telecom")
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.GetAlter
                , ConnectionHeader
                //, new Int32ValueHeader(HeaderId.SingleResponseMode, 0x01)
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/MAP-msg-listing")
                , new UnicodeStringValueHeader(HeaderId.Name, folderName)
                , new AppParamHeader(new AppParameter(AppParamTagId.MaxListCount, maxListCount))
                );

            Console.WriteLine($"Sending GetMessageListing request ");
            OBEXPacket resp = await RunObexRequest(packet);
            if (!(resp.Opcode.Equals(Opcode.Success) || resp.Opcode.Equals(Opcode.SuccessAlt)))
            {
                return null;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(((Utf8StringValueHeader)resp.Headers[HeaderId.EndOfBody]).Value);
            XmlNodeList list = xml.SelectNodes("/MAP-msg-listing/msg/@handle");
            List<string> ret = new List<string>();
            Console.WriteLine("Message handle list: ");
            foreach (XmlNode n in list)
            {
                if (n.Value != null)
                {
                    Console.WriteLine(n.Value);
                    ret.Add(n.Value);
                }
            }

            return ret;
        }

        public async Task<BMessage> GetMessage(string messageHandle)
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.GetAlter,
                ConnectionHeader,
                new AsciiStringValueHeader(HeaderId.Type, "x-bt/message"),
                new UnicodeStringValueHeader(HeaderId.Name, messageHandle),
                new AppParamHeader(
                    new AppParameter(AppParamTagId.Attachment, MasConstants.ATTACHMENT_ON),
                    new AppParameter(AppParamTagId.Charset, MasConstants.CHARSET_UTF8)
                    )
                );

            Console.WriteLine("Sending GetMessage request ");

            OBEXPacket resp = await RunObexRequest(packet);
            string bMsgStr = ((BodyHeader)resp.Headers[HeaderId.EndOfBody]).Value;

            BMessage bMsg = null;
            try
            {
                BMessageNode bMsgNode = BMessageNode.Parse(bMsgStr);
                bMsg = new BMessage();
                bMsg.Status = bMsgNode.Attributes["STATUS"] == "UNREAD" ? MessageStatus.UNREAD : MessageStatus.READ;
                bMsg.Type = bMsgNode.Attributes["TYPE"];
                bMsg.Folder = bMsgNode.Attributes["FOLDER"];
                bMsg.Sender = bMsgNode.ChildrenNode["VCARD"].Attributes["TEL"]; // TODO: parse vCard
                bMsg.Charset = bMsgNode.ChildrenNode["BENV"].ChildrenNode["BBODY"].Attributes["CHARSET"];
                bMsg.Length = int.Parse(bMsgNode.ChildrenNode["BENV"].ChildrenNode["BBODY"].Attributes["LENGTH"]);
                bMsg.Body = bMsgNode.ChildrenNode["BENV"].ChildrenNode["BBODY"].ChildrenNode["MSG"].Value;
            }
            catch (BMessageException)
            {
                Console.WriteLine("Invalid bmessage content");
                return null;
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return bMsg;
        }

        public async Task<bool> RemoteNotificationRegister()
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.PutAlter
                , ConnectionHeader
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/MAP-NotificationRegistration")
                , new AppParamHeader(new AppParameter(AppParamTagId.NotificationStatus, 1))
                , new BytesHeader(HeaderId.EndOfBody, 0x30)
                );

            Console.WriteLine("Sending RemoteNotificationRegister request");

            var status = await RunObexRequest(packet);

            return status.Equals(Opcode.Success) || status.Equals(Opcode.SuccessAlt);
        }

        public async Task<bool> GetMASInstanceInformation()
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.Get
                , ConnectionHeader
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/MASInstanceInformation")
                , new AppParamHeader(new AppParameter(AppParamTagId.MASInstanceID, MAS_UUID))
                );

            Console.WriteLine($"Sending GetMASInstanceInformation request ");
            OBEXPacket resp = await RunObexRequest(packet);
            return resp.Opcode.Equals(Opcode.Success) || resp.Opcode.Equals(Opcode.SuccessAlt);

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
                        string bodyString = null;
                        if (packet.Headers.ContainsKey(HeaderId.EndOfBody))
                        {
                            bodyString = ((BodyHeader)packet.Headers[HeaderId.EndOfBody]).Value;
                        }
                        else
                        {
                            Console.WriteLine("Recieved header dose not contains EndOfBody, abort! ");
                            continue;
                        }

                        printh(packet.Headers.Values);
                        Console.WriteLine("Body: " + bodyString);
                        XmlDocument doc = new XmlDocument();
                        //doc.LoadXml(bodyString + ">" /* Dont know why */ );
                        doc.LoadXml(bodyString);
                        string handle = doc.SelectSingleNode("/MAP-event-report/event/@handle").Value;

                        writer.WriteByte(0xA0); // Success
                        writer.WriteUInt16(3);
                        await writer.StoreAsync();


                        try
                        {
                            var mssgg = await GetMessage(handle);
                            Console.WriteLine($"Message received: {mssgg.Body.ToString()}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"GetMessage failed: {ex.Message}");
                            
                        }
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



        public async Task<List<string>> GetFolderList()
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.GetAlter
                , ConnectionHeader
                , new AsciiStringValueHeader(HeaderId.Type, "x-obex/folder-listing")
            //, new AppParamHeader(new AppParameter(AppParamTagId.MaxListCount, 100))
            );

            Console.WriteLine("sending GetFolderList request");

            OBEXPacket resp = await RunObexRequest(packet);
            if (!(resp.Opcode.Equals(Opcode.Success) || resp.Opcode.Equals(Opcode.SuccessAlt)))
            {
                return null;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(((Utf8StringValueHeader)resp.Headers[HeaderId.EndOfBody]).Value);
            XmlNodeList list = xml.SelectNodes("/folder-listing/folder/@name");
            List<string> ret = new List<string>();
            Console.WriteLine("Folder list: ");
            foreach (XmlNode n in list)
            {
                if (n.Value != null)
                {
                    Console.WriteLine(n.Value);
                    ret.Add(n.Value);
                }
            }

            return ret;
        }


        public async Task<bool> PushMessage()
        {
            OBEXPacket packet = new OBEXPacket(
                Opcode.PutAlter
                , ConnectionHeader
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/message")
                , new AsciiStringValueHeader(HeaderId.Name, "telecom/msg/inbox")
                //, new StringValueHeader(HeaderId.Name, "telecom/msg/inbox")
                //, new BytesHeader(HeaderId.SingleResponseMode, 0x01)
                , new AppParamHeader(new AppParameter(AppParamTagId.Charset, "native"))
                , new AsciiStringValueHeader(HeaderId.EndOfBody, "test pushing message from MCE")
                );

            Console.WriteLine("sending PushMessage request ");

            var status = await RunObexRequest(packet);
            return status.Equals(Opcode.Success) || status.Equals(Opcode.SuccessAlt);
        }

        private async Task<OBEXPacket> RunObexRequest(OBEXPacket req)
        {
            Opcode retOpc = Opcode.OBEX_UNAVAILABLE;
            OBEXPacket retPacket = null;

            Opcode srcOpc = req.Opcode;
            int c = 0;
            Opcode opc = Opcode.Continue;

            _writer = new DataWriter(BTSocket.OutputStream);
            _reader = new DataReader(BTSocket.InputStream);

            try
            {
                do
                {
                    Console.WriteLine($"Sending request packet: {++c}");
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
                        return new OBEXPacket { Opcode = retOpc };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Remote side disconnect: : {ex.Message}");
                        return new OBEXPacket { Opcode = retOpc };
                    }

                    try
                    {
                        Console.WriteLine($"Req packet sent. Waiting for reply...{c}");
                        retPacket = await OBEXPacket.ReadFromStream(_reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ReadFromStream exception {ex.Message}");
                        throw ex;
                    }

                    if (retPacket != null)
                    {
                        opc = retPacket.Opcode;
                        var bytes = retPacket.ToBuffer().ToArray();
                        Console.WriteLine("Reply packet:");
                        Console.WriteLine(BitConverter.ToString(bytes));
                        Console.WriteLine($"ResponseCode: {retPacket.Opcode}");

                        printh(retPacket.Headers.Values);

                        return retPacket;
                    }
                    else
                    {
                        return new OBEXPacket { Opcode = retOpc };
                    }

                    req = new OBEXPacket(srcOpc, ConnectionHeader);

                } while (c < 10
                           && (opc.Equals(Opcode.Continue) || opc.Equals(Opcode.ContinueAlt))
                        //&& (srcOpc.Equals(Opcode.Get) || srcOpc.Equals(Opcode.GetAlter))
                        );

            }
            catch (Exception ex)
            {
                Console.WriteLine("Remote request exception: " + ex.Message);
                return new OBEXPacket { Opcode = retOpc };
            }

            Console.WriteLine("Request returned success");
            return new OBEXPacket { Opcode = retPacket.Opcode };
        }

        private void printh(IEnumerable<IOBEXHeader> headers)
        {
            bool zeroFlag = true;
            foreach (var header in headers)
            {
                zeroFlag = false;
                Console.WriteLine($"{header.HeaderId}: {BitConverter.ToString(header.ToBytes())}");

                if (header.HeaderId.Equals(HeaderId.ApplicationParameters))
                {
                    var ap = (AppParamHeader)header;
                    foreach (var item in ap.AppParameters)
                    {
                        Console.WriteLine($"{item.TagId}: { BitConverter.ToString(item.Content)} ");
                    }
                    //break;
                }
            }
            if (headers != null && zeroFlag)
            {
                Console.WriteLine("No header returned.");
            }

        }
    }
}

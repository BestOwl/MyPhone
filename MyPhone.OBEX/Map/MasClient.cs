using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;

namespace MyPhone.OBEX.Map
{
    public class MasClient : ObexClient
    {

        //bb582b40-420c-11db-b0de-0800200c9a66
        public static readonly byte[] MAS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

        public MasClient(IInputStream inputStream, IOutputStream outputStream) : base(inputStream, outputStream)
        {
        }

        public async Task SetFolderAsync(SetPathMode mode, string folderName = "")
        {
            ObexPacket packet = new MapSetPathRequestPacket(mode, folderName);
            await RunObexRequest(packet);
        }

        /// <summary>
        /// Retrieve messages listing from MSE
        /// </summary>
        /// <param name="maxListCount">Maximum number of messages listed.</param>
        /// <param name="folderName">The name of the folder.</param>
        /// <returns>message handle list</returns>
        /// TODO: return Messages-Listing objects
        public async Task<List<string>> GetMessageListing(ushort maxListCount, string folderName = "telecom")
        {
            ObexPacket packet = new ObexPacket(
                Opcode.GetAlter
                //, new Int32ValueHeader(HeaderId.SingleResponseMode, 0x01)
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/MAP-msg-listing")
                , new UnicodeStringValueHeader(HeaderId.Name, folderName)
                , new AppParamHeader(new AppParameter(AppParamTagId.MaxListCount, maxListCount))
                );

            Console.WriteLine($"Sending GetMessageListing request ");
            ObexPacket resp = await RunObexRequest(packet);

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(((BodyHeader)resp.Headers[HeaderId.EndOfBody]).Value);
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

        public async Task<BMessage> GetMessageAsync(string messageHandle)
        {
            ObexPacket packet = new ObexPacket(
                Opcode.GetAlter,
                new AsciiStringValueHeader(HeaderId.Type, "x-bt/message"),
                new UnicodeStringValueHeader(HeaderId.Name, messageHandle),
                new AppParamHeader(
                    new AppParameter(AppParamTagId.Attachment, MasConstants.ATTACHMENT_ON),
                    new AppParameter(AppParamTagId.Charset, MasConstants.CHARSET_UTF8)
                    )
                );

            Console.WriteLine("Sending GetMessage request ");

            ObexPacket resp = await RunObexRequest(packet);

            // "EndOfBody" has been copied to "Body" by ObexClient
            string bMsgStr = ((BodyHeader)resp.Headers[HeaderId.Body]).Value!;

            BMessage bMsg;
            try
            {
                BMessageNode bMsgNode = BMessageNode.Parse(bMsgStr);
                bMsg = new BMessage(
                    status: bMsgNode.Attributes["STATUS"] == "UNREAD" ? MessageStatus.UNREAD : MessageStatus.READ,
                    type: bMsgNode.Attributes["TYPE"],
                    folder: bMsgNode.Attributes["FOLDER"],
                    charset: bMsgNode.ChildrenNode["BENV"].ChildrenNode["BBODY"].Attributes["CHARSET"],
                    length: int.Parse(bMsgNode.ChildrenNode["BENV"].ChildrenNode["BBODY"].Attributes["LENGTH"]),
                    body: bMsgNode.ChildrenNode["BENV"].ChildrenNode["BBODY"].ChildrenNode["MSG"].Value!,
                    sender: MixERP.Net.VCards.Deserializer.GetVCard(bMsgNode.ChildrenNode["VCARD"].ToString())
                    );
            }
            catch (BMessageException ex)
            {
                throw new ObexException($"Failed to get message (handle: {messageHandle}) from MSE. The MSE send back a invalid response", ex);
            }

            return bMsg;
        }

        public async Task SetNotificationRegistration(bool enableNotification)
        {
            byte flag = (byte)(enableNotification ? 1 : 0);

            ObexPacket packet = new ObexPacket(
                Opcode.PutAlter
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/MAP-NotificationRegistration")
                , new AppParamHeader(new AppParameter(AppParamTagId.NotificationStatus, flag))
                , new BytesHeader(HeaderId.EndOfBody, 0x30)
                );

            Console.WriteLine("Sending RemoteNotificationRegister request");
            await RunObexRequest(packet);
        }

        public async Task GetMASInstanceInformation()
        {
            ObexPacket packet = new ObexPacket(
                Opcode.Get
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/MASInstanceInformation")
                , new AppParamHeader(new AppParameter(AppParamTagId.MASInstanceID, MAS_UUID))
                );

            Console.WriteLine($"Sending GetMASInstanceInformation request ");
            await RunObexRequest(packet);
        }

        public async Task<List<string>> GetFolderList()
        {
            ObexPacket packet = new ObexPacket(
                Opcode.GetAlter
                , new AsciiStringValueHeader(HeaderId.Type, "x-obex/folder-listing")
            //, new AppParamHeader(new AppParameter(AppParamTagId.MaxListCount, 100))
            );

            Console.WriteLine("sending GetFolderList request");

            ObexPacket resp = await RunObexRequest(packet);

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(((BodyHeader)resp.Headers[HeaderId.EndOfBody]).Value);
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


        public async Task PushMessage()
        {
            ObexPacket packet = new ObexPacket(
                Opcode.PutAlter
                , new AsciiStringValueHeader(HeaderId.Type, "x-bt/message")
                , new AsciiStringValueHeader(HeaderId.Name, "telecom/msg/inbox")
                //, new StringValueHeader(HeaderId.Name, "telecom/msg/inbox")
                //, new BytesHeader(HeaderId.SingleResponseMode, 0x01)
                , new AppParamHeader(new AppParameter(AppParamTagId.Charset, MasConstants.CHARSET_UTF8))
                , new AsciiStringValueHeader(HeaderId.EndOfBody, "test pushing message from MCE")
                );

            Console.WriteLine("sending PushMessage request ");

            await RunObexRequest(packet);
        }

    }

}

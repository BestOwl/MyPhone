using GoodTimeStudio.MyPhone.OBEX.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Map
{
    public class MasClient : ObexClient
    {
        /// <remarks>
        /// Not null after connected.
        /// </remarks>
        private ObexHeader? _connectionIdHeader; 

        public MasClient(IInputStream inputStream, IOutputStream outputStream) : base(inputStream, outputStream)
        {
        }

        protected override void OnConnected(ObexPacket connectionResponse)
        {
            _connectionIdHeader = connectionResponse.GetHeader(HeaderId.ConnectionId);
        }

        public async Task SetFolderAsync(SetPathMode mode, string folderName = "")
        {
            ObexPacket packet = new MapSetPathRequestPacket(mode, folderName);
            packet.Headers[HeaderId.ConnectionId] = _connectionIdHeader!;
            await RunObexRequestAsync(packet);
        }

        /// <summary>
        /// Retrieve messages listing from MSE
        /// </summary>
        /// <param name="maxListCount">Maximum number of messages listed. Must be GREATER THAN 0.</param>
        /// <param name="folderName">The name of the folder.</param>
        /// <remarks>If you try to get the listing size, please call <see cref="GetMessageListingSizeAsync"/> instead.</remarks>
        /// <returns>message handle list</returns>
        /// TODO: return Messages-Listing objects
        public async Task<List<string>> GetMessagesListingAsync(ushort listStartOffset, ushort maxListCount, string folderName = "telecom")
        {
            ObexPacket packet = new ObexPacket(
                new ObexOpcode(ObexOperation.Get, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-bt/MAP-msg-listing", false, Encoding.UTF8),
                new ObexHeader(HeaderId.Name, folderName, true, Encoding.BigEndianUnicode),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)MasAppParamTagId.ListStartOffset, listStartOffset),
                    new AppParameter((byte)MasAppParamTagId.MaxListCount, maxListCount)).Build()
                );

            Console.WriteLine($"Sending GetMessageListing request ");
            ObexPacket resp = await RunObexRequestAsync(packet);

            XmlDocument xml = new XmlDocument();
            string listingObj = resp.GetBodyContentAsUtf8String(false);
            //string listingObj = System.Text.Encoding.UTF8.GetString(resp.BodyBuffer.ToArray());
            xml.LoadXml(listingObj);
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

        public async Task<ushort> GetMessageListingSizeAsync(string folderName = "telecom")
        {
            ObexPacket packet = new ObexPacket(
                new ObexOpcode(ObexOperation.Get, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-bt/MAP-msg-listing", true, Encoding.UTF8),
                new ObexHeader(HeaderId.Name, folderName, true, Encoding.BigEndianUnicode),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)MasAppParamTagId.MaxListCount, 0)).Build()
                );

            ObexPacket response = await RunObexRequestAsync(packet);
            AppParameterDictionary paramDict = response.GetHeader(HeaderId.ApplicationParameters)
                .GetValueAsAppParameters();
            return paramDict[(byte)MasAppParamTagId.ListingSize].GetValueAsUInt16();
        }

        public async Task<BMessage> GetMessageAsync(string messageHandle)
        {
            ObexPacket packet = new ObexPacket(
                new ObexOpcode(ObexOperation.Get, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-bt/message", true, Encoding.UTF8),
                new ObexHeader(HeaderId.Name, messageHandle, true, Encoding.BigEndianUnicode),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)MasAppParamTagId.Attachment, MasConstants.ATTACHMENT_ON),
                    new AppParameter((byte)MasAppParamTagId.Charset, MasConstants.CHARSET_UTF8)
                    ).Build()
                );

            Console.WriteLine("Sending GetMessage request ");

            ObexPacket resp = await RunObexRequestAsync(packet);

            string bMsgStr = resp.GetBodyContentAsUtf8String(true);

            BMessage bMsg;
            try
            {
                BMessageNode bMsgNode = BMessageNode.Parse(bMsgStr);
                bMsg = new BMessage(
                    status: bMsgNode.Attributes["STATUS"] == "UNREAD" ? BMessageStatus.UNREAD : BMessageStatus.READ,
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

        public async Task SetNotificationRegistrationAsync(bool enableNotification)
        {
            byte flag = (byte)(enableNotification ? 1 : 0);

            ObexPacket packet = new ObexPacket(
                new ObexOpcode(ObexOperation.Put, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-bt/MAP-NotificationRegistration", true, Encoding.UTF8),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)MasAppParamTagId.NotificationStatus, flag)).Build(),
                new ObexHeader(HeaderId.EndOfBody, 0x30)
                );

            Console.WriteLine("Sending RemoteNotificationRegister request");
            await RunObexRequestAsync(packet);
        }

        public async Task GetMasInstanceInformationAsync()
        {
            ObexPacket packet = new ObexPacket(
                new(ObexOperation.Get, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-bt/MASInstanceInformation", true, Encoding.UTF8),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)MasAppParamTagId.MASInstanceID, ObexServiceUuid.MessageAccess.Value)).Build()
                );

            Console.WriteLine($"Sending GetMASInstanceInformation request ");
            await RunObexRequestAsync(packet);
        }

        /// <summary>
        /// Get the list of children folder name in the current folder
        /// </summary>
        /// <param name="maxListCount">The maximum number of folders to retrieve (default 1024).</param>
        /// <param name="listStartOffset">The offset of the first entry of the returned folder</param>
        /// <returns>List of children folder name</returns>
        public async Task<List<string>> GetFolderListingAsync(ushort? maxListCount = null, ushort? listStartOffset = null)
        {
            ObexPacket packet = new ObexPacket(
                new(ObexOperation.Get, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-obex/folder-listing", true, Encoding.UTF8)
            );
            if (maxListCount != null || listStartOffset != null)
            {
                AppParameterHeaderBuilder builder = new();
                if (maxListCount != null)
                {
                    builder.AppParameters.Add(new AppParameter((byte)MasAppParamTagId.MaxListCount, maxListCount.Value));
                }
                if (listStartOffset != null)
                {
                    builder.AppParameters.Add(new AppParameter((byte)MasAppParamTagId.ListStartOffset, listStartOffset.Value));
                }
                packet.Headers[HeaderId.ApplicationParameters] = builder.Build();
            }

            Console.WriteLine("sending GetFolderList request");

            ObexPacket resp = await RunObexRequestAsync(packet);

            XmlDocument xml = new XmlDocument();
            string objStr = resp.GetBodyContentAsUtf8String(true);
            xml.LoadXml(objStr);
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
                new ObexOpcode(ObexOperation.Put, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Type, "x-bt/message", true, Encoding.UTF8),
                new ObexHeader(HeaderId.Type, "telecom/msg/inbox", true, Encoding.BigEndianUnicode),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)MasAppParamTagId.Charset, MasConstants.CHARSET_UTF8)).Build(),
                new ObexHeader(HeaderId.EndOfBody, "test pushing message from MCE", true, Encoding.UTF8)
                );

            Console.WriteLine("sending PushMessage request ");

            await RunObexRequestAsync(packet);
        }

        /// <summary>
        /// Get all children folders' name of current folder
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<List<string>> GetAllChildrenFoldersAsync()
        {
            List<string> ret = new();

            bool lastPage = false;
            ushort offset = 0;
            const int default_size = 1024;
            while (!lastPage)
            {
                var foldersName = await GetFolderListingAsync(listStartOffset: offset);
                if (foldersName.Count < default_size)
                {
                    lastPage = true;
                }

                ret.AddRange(foldersName);
            }

            return ret;
        }

        public async Task<List<string>> GetAllMessagesAsync(string folderName = "telecom")
        {
            List<string> ret = new();
            bool lastPage = false;
            ushort offset = 0;
            const int default_size = 1024;
            while (!lastPage)
            {
                var handles = await GetMessagesListingAsync(offset, default_size, folderName);
                if (handles.Count < default_size)
                {
                    lastPage = true;
                }
                ret.AddRange(handles);
            }

            return ret;
        }

        /// <summary>
        /// Traverese the entire folder tree.
        /// </summary>
        /// <returns>Root folder and children folder</returns>
        public async Task<SmsFolder> TraverseFolderAsync(bool getMessageHandles = false)
        {
            await SetFolderAsync(SetPathMode.BackToRoot);

            SmsFolder root = new SmsFolder("Root");
            Stack<SmsFolder> folders = new Stack<SmsFolder>();
            folders.Push(root);
            SmsFolder pre = root;

            while (folders.Count > 0)
            {
                SmsFolder current = folders.Pop();

                if (current != root && current.Parent != pre.Parent)
                {
                    if (current.Parent == pre)
                    {
                        await SetFolderAsync(SetPathMode.EnterFolder, current.Name);
                    }
                    else if (pre.Parent != null && pre.Parent.Parent == current.Parent)
                    {
                        await SetFolderAsync(SetPathMode.BackToParent);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unreachable code reached!");
                    }
                }

                List<string> subFoldersName = await GetAllChildrenFoldersAsync();
                foreach (string subFolder in subFoldersName)
                {
                    ushort count = await GetMessageListingSizeAsync(subFolder);
                    SmsFolder smsFolder = new SmsFolder(subFolder, count, current);
                    if (getMessageHandles)
                    {
                        foreach (string handle in await GetAllMessagesAsync(subFolder))
                        {
                            smsFolder.MessageHandles.Add(handle);
                        }
                    }

                    current.Children.Add(smsFolder);
                    folders.Push(smsFolder);
                }

                pre = current;
            }

            return root;
        }

    }
}

using System;
using System.Xml;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX
{
    public class MessageReceivedEventArgs
    {
        public string MessageHandle { get; set; }

        public MessageReceivedEventArgs(string messageHandle)
        {
            MessageHandle = messageHandle;
        }
    }

    public class MnsServer : ObexServer
    {
        public MnsServer(IInputStream inputStream, IOutputStream outputStream) : base(inputStream, outputStream, ObexServiceUuid.MessageNotification)
        { }

        public delegate void MnsMessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public event MnsMessageReceivedEventHandler? MessageReceived;

        protected override ObexPacket? OnClientRequest(ObexPacket clientRequestPacket)
        {
            Console.WriteLine("Opcode: " + clientRequestPacket.Opcode);

            if (clientRequestPacket.Opcode.ObexOperation == ObexOperation.Put)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(clientRequestPacket.GetBodyContentAsUtf8String(true));
                string? handle = doc.SelectSingleNode("/MAP-event-report/event/@handle")?.Value;
                
                if (handle != null)
                {
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(handle));
                    return new ObexPacket(new ObexOpcode(ObexOperation.Success, true));
                }
            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
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
        public MnsServer(IInputStream inputStream, IOutputStream outputStream) : base(inputStream, outputStream)
        { }

        public delegate void MnsMessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public event MnsMessageReceivedEventHandler? MessageReceived;

        protected override ObexPacket? OnClientRequest(ObexPacket clientRequestPacket)
        {
            Console.WriteLine("Opcode: " + clientRequestPacket.Opcode);

            if (clientRequestPacket.Opcode == Opcode.Put || clientRequestPacket.Opcode == Opcode.PutAlter)
            {
                string bodyString;
                if (clientRequestPacket.Headers.ContainsKey(HeaderId.EndOfBody))
                {
                    bodyString = ((BodyHeader)clientRequestPacket.Headers[HeaderId.EndOfBody]).Value!;
                }
                else
                {
                    Console.WriteLine("Recieved header dose not contains EndOfBody, abort! ");
                    return null;
                }

                clientRequestPacket.PrintHeaders();
                Console.WriteLine("Body: " + bodyString);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(bodyString);
                string handle = doc.SelectSingleNode("/MAP-event-report/event/@handle").Value;
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(handle));

                return new ObexPacket(Opcode.Success);
            }

            return null;
        }
    }
}

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class ObexServer
    {
        private DataReader _reader;
        private DataWriter _writer;

        private CancellationTokenSource _cts;

        public ObexServer(IInputStream inputStream, IOutputStream outputStream)
        {
            _reader = new DataReader(inputStream);
            _writer = new DataWriter(outputStream);
            _cts = new CancellationTokenSource();
        }

        public void StartServer()
        {
            Task.Run(async () =>
            {
                OBEXPacket packet = await OBEXPacket.ReadFromStream(_reader, new OBEXConnectPacket());
                if (packet.Opcode != Opcode.Connect)
                {
                    Console.WriteLine("Not support operation code: " + packet.Opcode);
                    Console.WriteLine("MSE should send Connect request first");
                    return;
                }

                packet.Opcode = Opcode.Success;
                _writer.WriteBuffer(packet.ToBuffer());
                await _writer.StoreAsync();

                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    packet = await OBEXPacket.ReadFromStream(_reader);

                    OBEXPacket? response = OnClientRequest(packet);
                    if (response != null)
                    {
                        _writer.WriteBuffer(response.ToBuffer());
                        await _writer.StoreAsync();
                    }
                    else
                    {
                        _writer.WriteByte(0xC6); // Not Acceptable
                        _writer.WriteUInt16(3);
                        await _writer.StoreAsync();
                    }
                }
            }, _cts.Token);
        }

        public void StopServer()
        {
            _cts.Cancel();
        }

        /// <summary>
        /// Handle client request.
        /// </summary>
        /// <remarks>
        /// This method will be called whenever a client request arrived.
        /// </remarks>
        /// <param name="clientRequestPacket"></param>
        /// <returns>
        /// The OBEX response packet. If the server doesn't know how to handle this client request, return null.
        /// </returns>
        protected virtual OBEXPacket? OnClientRequest(OBEXPacket clientRequestPacket)
        {
            return null;
        }
    }
}

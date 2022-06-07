using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class ObexServer
    {
        private DataReader _reader;
        private DataWriter _writer;

        private ObexServiceUuid _serviceUuid;

        private CancellationTokenSource _cts;

        public ObexServer(IInputStream inputStream, IOutputStream outputStream, ObexServiceUuid serviceUuid)
        {
            _reader = new DataReader(inputStream);
            _writer = new DataWriter(outputStream);
            _serviceUuid = serviceUuid;
            _cts = new CancellationTokenSource();
        }

        public async Task Run()
        {
            while (true)
            {
                _cts.Token.ThrowIfCancellationRequested();

                ObexPacket packet = await ObexPacket.ReadFromStream(_reader, new ObexConnectPacket());
                if (packet.Opcode == Opcode.Connect)
                {
                    if (packet.Headers.ContainsKey(HeaderId.Target))
                    {
                        BytesHeader targetHeader = (BytesHeader)packet.Headers[HeaderId.Target];
                        if (targetHeader.Value != null)
                        {
                            if (Enumerable.SequenceEqual(targetHeader.Value, _serviceUuid.Value))
                            {
                                packet.Opcode = Opcode.Success;
                                _writer.WriteBuffer(packet.ToBuffer());
                                await _writer.StoreAsync();
                                break;
                            }
                        }
                    }

                }

                Console.WriteLine("Not support operation code: " + packet.Opcode);
                Console.WriteLine("MSE should send Connect request first");
                packet = new ObexPacket(Opcode.OBEX_UNAVAILABLE);
                _writer.WriteBuffer(packet.ToBuffer());
            }

            while (true)
            {
                _cts.Token.ThrowIfCancellationRequested();

                ObexPacket packet = await ObexPacket.ReadFromStream(_reader);

                ObexPacket? response = OnClientRequest(packet);
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
        protected virtual ObexPacket? OnClientRequest(ObexPacket clientRequestPacket)
        {
            return null;
        }
    }
}

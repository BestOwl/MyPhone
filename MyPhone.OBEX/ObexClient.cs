using GoodTimeStudio.MyPhone.OBEX.Headers;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX
{
    public class ObexClient
    {
        private DataReader _reader;
        private DataWriter _writer;

        public bool Conntected { get; private set; } = false;

        public ObexClient(IInputStream inputStream, IOutputStream outputStream)
        {
            _reader = new DataReader(inputStream);
            _writer = new DataWriter(outputStream);
        }

        /// <summary>
        /// Send OBEX Connect packet to the server.
        /// </summary>
        /// <param name="targetUuid">A 16-length byte array indicates the UUID of the target service.</param>
        /// <exception cref="InvalidOperationException">The Connect method can call only once and it is already called before.</exception>
        /// <exception cref="ObexExceptions">The request failed due to an underlying issue such as connection issue, or the server reply with a invalid response</exception>
        public async Task ConnectAsync(ObexServiceUuid targetService)
        {
            if (Conntected)
            {
                throw new InvalidOperationException("ObexClient is already connected to a ObexServer");
            }

            ObexConnectPacket packet = new ObexConnectPacket(targetService);
            var buf = packet.ToBuffer();

            Console.WriteLine("Sending OBEX Connection request to server:");
            Console.WriteLine(BitConverter.ToString(buf.ToArray()));
            Console.WriteLine("Opcode: " + packet.Opcode);

            _writer.WriteBuffer(buf);
            await _writer.StoreAsync();

            Console.WriteLine("Waiting reply packet...");
            ObexConnectPacket response = await ObexPacket.ReadFromStream<ObexConnectPacket>(_reader);

            var bytes = response.ToBuffer().ToArray();
            Console.WriteLine("Reply packet:");
            Console.WriteLine(BitConverter.ToString(bytes));
            Console.WriteLine($"ResponseCode: {response.Opcode}");

            if (response.Opcode.ObexOperation != ObexOperation.Success)
            {
                throw new ObexRequestException(response.Opcode, $"Unable to connect to the target OBEX service.");
            }

            Conntected = true;
            OnConnected(response);
        }

        protected virtual void OnConnected(ObexPacket connectionResponse) { }

        public Task DisconnectAsync()
        {
            if (!Conntected)
            {
                throw new InvalidOperationException("ObexClient is not connected to any ObexServer");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Send OBEX request to MSE
        /// </summary>
        /// <param name="req">The request packet</param>
        /// <returns>Response packet. The resposne packet is null if the MSE did not send back any response, or the response is corrupted</returns>
        /// <exception cref="ObexRequestException">Throws if get an valid response, but its opcode is unsuccessful</exception>
        /// <exception cref="ObexException"> due to an underlying issue such as connection loss, invalid server response</exception>
        public async Task<ObexPacket> RunObexRequestAsync(ObexPacket req)
        {
            if (!Conntected)
            {
                throw new InvalidOperationException("ObexClient is not connected to any ObexServer");
            }

            ObexOperation? requestOperation = req.Opcode.ObexOperation;
            if (requestOperation == null)
            {
                throw new InvalidOperationException("User-defined opcode is not supported");
            }

            ObexPacket? response = null;
            int c = 0;

            using (MemoryStream bodyMemoryStream = new MemoryStream())
            {
                do
                {
                    Console.WriteLine($"Sending request packet: {++c}");
                    var buf = req.ToBuffer();
                    Console.WriteLine("Opcode: " + req.Opcode);
                    _writer.WriteBuffer(buf);
                    await _writer.StoreAsync();

                    ObexPacket subResponse;
                    subResponse = await ObexPacket.ReadFromStream(_reader);
                    Console.WriteLine($"ResponseCode: {subResponse.Opcode}");

                    if (response == null)
                    {
                        response = subResponse;
                    }

                    switch (subResponse.Opcode.ObexOperation)
                    {
                        case ObexOperation.Success:
                            if (subResponse.Headers.TryGetValue(HeaderId.EndOfBody, out ObexHeader? endOfBodyHeader))
                            {
                                bodyMemoryStream.Write(endOfBodyHeader.Buffer);
                            }
                            response.Opcode = subResponse.Opcode;
                            response.BodyBuffer = bodyMemoryStream.ToArray();
                            return response;
                        case ObexOperation.Continue:
                            if (subResponse.Headers.TryGetValue(HeaderId.Body, out ObexHeader? bodyHeader))
                            {
                                bodyMemoryStream.Write(bodyHeader.Buffer);
                            }
                            break;
                        default:
                            throw new ObexRequestException(subResponse.Opcode, $"The {requestOperation} request failed with opcode {subResponse.Opcode}");
                    }

                    req = new ObexPacket(new ObexOpcode(requestOperation.Value, true));
                } while (true);
            }
        }
    }
}

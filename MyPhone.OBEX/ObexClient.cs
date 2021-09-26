using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class ObexClient
    {
        private DataReader _reader;
        private DataWriter _writer;

        private Int32ValueHeader? _connectionIdHeader;

        public bool Conntected { get; set; } = false;

        public ObexClient(IInputStream inputStream, IOutputStream outputStream)
        {
            _reader = new DataReader(inputStream);
            _writer = new DataWriter(outputStream);
        }

        public async Task Connect()
        {
            if (Conntected)
            {
                throw new InvalidOperationException("ObexClient is already connected to a ObexServer");
            }

            ObexConnectPacket packet = new ObexConnectPacket();
            var buf = packet.ToBuffer();

            Console.WriteLine("Sending OBEX Connection request to server:");
            Console.WriteLine(BitConverter.ToString(buf.ToArray()));
            Console.WriteLine("Opcode: " + packet.Opcode);

            _writer.WriteBuffer(buf);
            await _writer.StoreAsync();

            Console.WriteLine("Waiting reply packet...");
            ObexPacket response = await ObexPacket.ReadFromStream(_reader, packet);

            var bytes = response.ToBuffer().ToArray();
            Console.WriteLine("Reply packet:");
            Console.WriteLine(BitConverter.ToString(bytes));
            Console.WriteLine($"ResponseCode: {response.Opcode}");

            _connectionIdHeader = (Int32ValueHeader)response.Headers[HeaderId.ConnectionId];
            Conntected = true;
        }

        public async Task Disconnect()
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
        /// <exception cref="ObexRequestException"> due to an underlying issue such as connection loss, invalid server response</exception>
        public async Task<ObexPacket> RunObexRequest(ObexPacket req)
        {
            if (!Conntected)
            {
                throw new InvalidOperationException("ObexClient is not connected to any ObexServer");
            }

            Opcode requestOpcode = req.Opcode;
            ObexPacket? response = null;
            int c = 0;

            do
            {
                Console.WriteLine($"Sending request packet: {++c}");
                req.Headers[HeaderId.ConnectionId] = _connectionIdHeader!;
                var buf = req.ToBuffer();
                Console.WriteLine(BitConverter.ToString(buf.ToArray()));
                Console.WriteLine("Opcode: " + req.Opcode);
                _writer.WriteBuffer(buf);
                await _writer.StoreAsync();

                ObexPacket subResponse;
                subResponse = await ObexPacket.ReadFromStream(_reader);

                var bytes = subResponse.ToBuffer().ToArray();
                Console.WriteLine("Reply packet:");
                Console.WriteLine(BitConverter.ToString(bytes));
                Console.WriteLine($"ResponseCode: {subResponse.Opcode}");

                subResponse.PrintHeaders();
                if (response == null)
                {
                    response = subResponse;
                }

                switch (subResponse.Opcode)
                {
                    case Opcode.Success:
                    case Opcode.SuccessAlt:
                        if (subResponse.Headers.ContainsKey(HeaderId.EndOfBody))
                        {
                            if (response.Headers.ContainsKey(HeaderId.Body))
                            {
                                ((BodyHeader)response.Headers[HeaderId.Body]).Value += ((BodyHeader)subResponse.Headers[HeaderId.EndOfBody]).Value;
                            }
                            else
                            {
                                response.Headers[HeaderId.Body] = response.Headers[HeaderId.EndOfBody];
                            }
                        }
                        return response;
                    case Opcode.Continue:
                    case Opcode.ContinueAlt:
                        if (response != subResponse)
                        {
                            if (response.Headers.ContainsKey(HeaderId.Body))
                            {
                                ((BodyHeader)response.Headers[HeaderId.Body]).Value += ((BodyHeader)subResponse.Headers[HeaderId.Body]).Value;
                            }
                        }
                        break;
                    default:
                        throw new ObexRequestException($"The {requestOpcode} request failed with opcode ${subResponse.Opcode}");
                }
                
                req = new ObexPacket(requestOpcode, _connectionIdHeader!);
            } while (c < 10);
      
            Console.WriteLine("Maultiple GET over 10 times, abort!");
            return response;
        }
    }

    public class ObexRequestException : Exception
    {
        /// <summary>
        /// Represents a non-successful response opcode
        /// </summary>
        public Opcode? Opcode { get; set; }

        public ObexRequestException()
        {
        }

        public ObexRequestException(string message) : base(message)
        {
        }

        public ObexRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ObexRequestException(string message, Opcode opcode) : base(message)
        {
            Opcode = opcode;
        }
    }
}

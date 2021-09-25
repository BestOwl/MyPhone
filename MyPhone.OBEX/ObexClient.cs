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

            OBEXConnectPacket packet = new OBEXConnectPacket();
            var buf = packet.ToBuffer();

            Console.WriteLine("Sending OBEX Connection request to server:");
            Console.WriteLine(BitConverter.ToString(buf.ToArray()));
            Console.WriteLine("Opcode: " + packet.Opcode);

            _writer.WriteBuffer(buf);
            await _writer.StoreAsync();

            Console.WriteLine("Waiting reply packet...");
            OBEXPacket response = await OBEXPacket.ReadFromStream(_reader, packet);

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
        public async Task<OBEXPacket> RunObexRequest(OBEXPacket req)
        {
            if (!Conntected)
            {
                throw new InvalidOperationException("ObexClient is not connected to any ObexServer");
            }

            Opcode requestOpcode = req.Opcode;
            OBEXPacket response;
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

                response = await OBEXPacket.ReadFromStream(_reader);

                var bytes = response.ToBuffer().ToArray();
                Console.WriteLine("Reply packet:");
                Console.WriteLine(BitConverter.ToString(bytes));
                Console.WriteLine($"ResponseCode: {response.Opcode}");

                response.PrintHeaders();
                
                if (response.Opcode == Opcode.Success || response.Opcode == Opcode.SuccessAlt)
                {
                    return response;
                }
                else if (response.Opcode != Opcode.Continue || response.Opcode != Opcode.ContinueAlt)
                {
                    throw new ObexRequestException($"The {requestOpcode} request failed with opcode ${response.Opcode}");
                }

                req = new OBEXPacket(requestOpcode, _connectionIdHeader!);
            } while (c < 10);

      
            Console.WriteLine("Request returned success");
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

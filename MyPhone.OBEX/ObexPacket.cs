using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class ObexPacket
    {
        public Opcode Opcode { get; set; }

        /// <summary>
        /// Will only be updated after calling ToBuffer()
        /// </summary>
        public ushort PacketLength { get; set; }

        public Dictionary<HeaderId, IObexHeader> Headers;

        public ObexPacket(Opcode op)
        {
            Opcode = op;
            Headers = new Dictionary<HeaderId, IObexHeader>();
        }

        public ObexPacket(Opcode op, params IObexHeader[] headers) : this(op)
        {
            foreach (IObexHeader h in headers)
            {
                Headers[h.HeaderId] = h;
            }
        }

        protected virtual void WriteExtraField(DataWriter writer) { }

        /// <summary>
        /// Read extra field after the length field
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The number of bits required for extra bits</returns>
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        protected virtual async Task<uint> ReadExtraField(DataReader reader)
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            return 0;
        }

        public IBuffer ToBuffer()
        {
            DataWriter writer = new DataWriter();
            DataWriter exFieldAndHeaderWriter = new DataWriter();

            WriteExtraField(exFieldAndHeaderWriter);

            foreach (IObexHeader header in Headers.Values)
            {
                exFieldAndHeaderWriter.WriteByte((byte)header.HeaderId);
                byte[]? content = header.ToBytes();
                if (content != null)
                {
                    if (header.HeaderId.Equals(HeaderId.ConnectionId))
                    {
                        Console.WriteLine($"ConnectionId: {BitConverter.ToString(content)}");
                    }

                    if (header.GetFixedLength() == 0)
                    {
                        exFieldAndHeaderWriter.WriteUInt16((ushort)(content.Length + sizeof(HeaderId) + sizeof(ushort)));
                    }
                    exFieldAndHeaderWriter.WriteBytes(content);
                }
                else
                {
                    exFieldAndHeaderWriter.WriteUInt16(0);
                }
            }

            IBuffer exFieldAndHeaderBuffer = exFieldAndHeaderWriter.DetachBuffer();

            writer.WriteByte((byte)Opcode);
            PacketLength = (ushort)(exFieldAndHeaderBuffer.Length + sizeof(Opcode) + sizeof(ushort));
            writer.WriteUInt16(PacketLength);
            writer.WriteBuffer(exFieldAndHeaderBuffer);

            return writer.DetachBuffer();
        }

        private async Task ParseHeader(DataReader reader, uint headerSize)
        {
            if (headerSize <= 0)
            {
                Console.WriteLine("Header size to read is zero.");
                return;
            }

            uint loaded = await reader.LoadAsync(headerSize);
            if (loaded <= 0)
            {
                throw new ObexRequestException($"No data returned for {headerSize} unint read.");
            }

            while (true)
            {
                if (reader.UnconsumedBufferLength == 0)
                {
                    break;
                }

                byte read = reader.ReadByte();
                Console.WriteLine(read);

                IObexHeader header = ObexHeaderFromByte(read);

                if (header != null)
                {

                    ushort len = header.GetFixedLength();
                    if (len == 0)
                    {
                        len = (ushort)(reader.ReadUInt16() - sizeof(HeaderId) - sizeof(ushort));
                    }

                    if (len == 0)
                    {
                        continue;
                    }

                    byte[] b = new byte[len];
                    reader.ReadBytes(b);
                    header.FromBytes(b);
                    Headers[header.HeaderId] = header;
                }
            }
        }

        public void PrintHeaders()
        {
            bool zeroFlag = true;
            foreach (var header in Headers.Values)
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
            if (zeroFlag)
            {
                Console.WriteLine("No header returned.");
            }
        }

        /// <summary>
        /// Read and parse OBEX packet from DataReader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="packet">Optional, if this parameter is not null, the data will be read into this parameter</param>
        /// <returns>Loaded OBEX packet</returns>
        public async static Task<ObexPacket> ReadFromStream(DataReader reader, ObexPacket? packet = null)
        {
            uint loaded = await reader.LoadAsync(1);
            if (loaded != 1)
            {
                throw new ObexRequestException("The underlying socket was closed before we were able to read the whole data.");
            }

            Opcode opcode = (Opcode)reader.ReadByte();
            if (packet == null)
            {
                packet = new ObexPacket(opcode);
            }
            else
            {
                packet.Opcode = opcode;
            }
            Console.WriteLine($"ReadFromStream:: Opcode: {packet.Opcode}");

            loaded = await reader.LoadAsync(sizeof(ushort));
            if (loaded != sizeof(ushort))
            {
                throw new ObexRequestException("The underlying socket was closed before we were able to read the whole data.");
            }

            packet.PacketLength = reader.ReadUInt16();
            Console.WriteLine($"packet length: {packet.PacketLength}");

            uint extraFieldBits = await packet.ReadExtraField(reader);
            uint size = packet.PacketLength - (uint)sizeof(Opcode) - sizeof(ushort) - extraFieldBits;
            await packet.ParseHeader(reader, size);
            return packet;
        }

        public static IObexHeader ObexHeaderFromByte(byte b)
        {
            HeaderId headerId = (HeaderId)b;
            IObexHeader header;
            switch (headerId)
            {
                case HeaderId.ConnectionId:
                case HeaderId.SingleResponseMode:
                    header = new Int32ValueHeader(headerId);
                    break;
                case HeaderId.ApplicationParameters:
                    header = new AppParamHeader();
                    break;
                case HeaderId.Type:
                case HeaderId.Name:
                    header = new Utf8StringValueHeader(headerId);
                    break;
                case HeaderId.EndOfBody:
                case HeaderId.Body:
                    header = new BodyHeader(headerId);
                    break;
                case HeaderId.Who:
                case HeaderId.Target:
                    header = new BytesHeader(headerId);
                    break;
                default:
                    //throw new NotSupportedException($"Input byte '{b}' does not match HeaderId definition. ");
                    Console.WriteLine($"Input byte '{b}' does not match HeaderId definition. ");
                    header = new AsciiStringValueHeader(headerId);
                    break;
            }

            return header;
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class OBEXPacket
    {
        public Opcode Opcode { get; set; }

        /// <summary>
        /// Will only be updated after calling ToBuffer()
        /// </summary>
        public ushort PacketLength { get; set; }

        public LinkedList<IOBEXHeader> Headers;

        public OBEXPacket()
        {
            Headers = new LinkedList<IOBEXHeader>();
        }

        public OBEXPacket(params IOBEXHeader[] headers) : this()
        {
            foreach (IOBEXHeader h in headers)
            {
                Headers.AddLast(h);
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

            foreach (IOBEXHeader header in Headers)
            {
                exFieldAndHeaderWriter.WriteByte((byte)header.HeaderId);
                byte[] content = header.ToBytes();
                if (header.GetFixedLength() == 0)
                {
                    exFieldAndHeaderWriter.WriteUInt16((ushort)(content.Length + sizeof(HeaderId) + sizeof(ushort)));
                }
                exFieldAndHeaderWriter.WriteBytes(content);
            }

            IBuffer exFieldAndHeaderBuffer = exFieldAndHeaderWriter.DetachBuffer();

            writer.WriteByte((byte)Opcode);
            PacketLength = (ushort)(exFieldAndHeaderBuffer.Length + sizeof(Opcode) + sizeof(ushort));
            writer.WriteUInt16(PacketLength);
            writer.WriteBuffer(exFieldAndHeaderBuffer);

            return writer.DetachBuffer();
        }

        public static Task<OBEXPacket> ReadFromStream(DataReader reader)
        {
            return ReadFromStream(reader, new OBEXPacket());
        }

        public async static Task<OBEXPacket> ReadFromStream(DataReader reader, OBEXPacket packet)
        {
            uint loaded = await reader.LoadAsync(1);
            if (loaded <= 0)
            {
                goto fail;
            }

            packet.Opcode = (Opcode)reader.ReadByte();

            loaded = await reader.LoadAsync(2);
            if (loaded <= 0)
            {
                goto fail;
            }
            packet.PacketLength = reader.ReadUInt16();
            uint extraFieldBits = await packet.ReadExtraField(reader);
            uint size = packet.PacketLength - (uint)sizeof(Opcode) - sizeof(ushort) - extraFieldBits;
            await packet.ParseHeader(reader, size);
            return packet;

        fail:
            return null;
        }

        private async Task ParseHeader(DataReader reader, uint headerSize)
        {
            if (headerSize <= 0)
            {
                return;
            }

            uint loaded = await reader.LoadAsync(headerSize);
            if (loaded <= 0)
            {
                return;
            }

            while (true)
            {
                if (reader.UnconsumedBufferLength == 0)
                {
                    break;
                }

                HeaderId headerId = (HeaderId)reader.ReadByte();
                IOBEXHeader header = null;
                switch (headerId)
                {
                    case HeaderId.ConnectionId:
                        header = new Int32ValueHeader(headerId);
                        break;
                    case HeaderId.ApplicationParameters:
                        header = new AppParamHeader();
                        break;
                    case HeaderId.Type:
                    case HeaderId.Name:
                    case HeaderId.EndOfBody:
                    case HeaderId.Body:
                        header = new StringValueHeader(headerId);
                        break;
                    case HeaderId.Who:
                    case HeaderId.Target:
                        header = new BytesHeader(headerId);
                        break;
                    default:
                        throw new NotSupportedException("Not supprted header id: " + headerId);
                }

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
                Headers.AddLast(header);
            }
        }
    }
}

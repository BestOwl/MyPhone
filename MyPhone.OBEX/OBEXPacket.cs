using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class OBEXPacket
    {
        public virtual Opcode Opcode { get; set; }

        /// <summary>
        /// Will only be updated after calling ToBuffer()
        /// </summary>
        public ushort PacketLength { get; set; }

        public LinkedList<OBEXHeader> Headers;

        public OBEXPacket()
        {
            Headers = new LinkedList<OBEXHeader>();
        }

        protected virtual void WriteExtraField(DataWriter writer) 
        { 
        }

        public IBuffer ToBuffer()
        {
            DataWriter writer = new DataWriter();
            DataWriter exFieldAndHeaderWriter = new DataWriter();
            
            WriteExtraField(exFieldAndHeaderWriter);

            foreach (OBEXHeader header in Headers)
            {
                exFieldAndHeaderWriter.WriteByte((byte)header.HeaderId);
                if (header is ILengthRequiredHeader iLen)
                {
                    exFieldAndHeaderWriter.WriteUInt16(iLen.GetHeaderLength());
                }
                exFieldAndHeaderWriter.WriteBytes(header.ToBytes());
            }

            IBuffer exFieldAndHeaderBuffer = exFieldAndHeaderWriter.DetachBuffer();

            writer.WriteByte((byte)Opcode);
            PacketLength = (ushort)(exFieldAndHeaderBuffer.Length + sizeof(Opcode) + sizeof(ushort));
            writer.WriteUInt16(PacketLength);
            writer.WriteBuffer(exFieldAndHeaderBuffer);

            return writer.DetachBuffer();
        }

        public async static Task<OBEXPacket> ReadFromStream(DataReader reader)
        {
            uint loaded = await reader.LoadAsync(1);
            if (loaded <= 0)
            {
                goto fail;
            }

            OBEXPacket ret = new OBEXPacket();
            ret.Opcode = (Opcode) reader.ReadByte();

            loaded = await reader.LoadAsync(2);
            if (loaded <= 0)
            {
                goto fail;
            }
            ret.PacketLength = reader.ReadUInt16();
            ret.ParseHeader(reader);
            return ret;

            fail:
            Console.WriteLine("Failed.");
            return null;
        }

        private async void ParseHeader(DataReader reader)
        {
            uint size = PacketLength - (uint)sizeof(Opcode) - sizeof(ushort);
            if (size <= 0)
            {
                return;
            }

            uint loaded = await reader.LoadAsync(size);
            if (loaded <= 0)
            {
                return;
            }
            //while (true)
            //{

            //}
        }
    }
}

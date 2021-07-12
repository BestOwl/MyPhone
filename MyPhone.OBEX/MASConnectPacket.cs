using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class MASConnectPacket : OBEXPacket
    {
        public static readonly byte[] MAS_UUID = new byte[] { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

        public override Opcode Opcode => Opcode.Connect;

        public static byte OBEXVersion = 0x10;

        public static byte Flags = 0;

        public static readonly ushort MaximumPacketLength = 0xFFFF;

        public MASConnectPacket()
        {
            Headers.AddLast(new BytesHeader(HeaderId.Target, MAS_UUID));
        }

        protected override void WriteExtraField(DataWriter writer)
        {
            writer.WriteByte(OBEXVersion);
            writer.WriteByte(Flags);
            writer.WriteUInt16(MaximumPacketLength);
        }
    }
}

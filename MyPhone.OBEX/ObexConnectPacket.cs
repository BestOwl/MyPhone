using GoodTimeStudio.MyPhone.OBEX.Headers;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX
{
    public class ObexConnectPacket : ObexPacket
    {
        public static readonly uint _EXTRA_FIELD_BITS = sizeof(byte) * 2 + sizeof(ushort);

        public byte OBEXVersion = 0x10;

        public byte Flags = 0;

        public ushort MaximumPacketLength = 0xFFFF;

        public bool Disconnect { get; set; } = false;

        /// <summary>
        /// Create a empty instance ready for reading content from stream
        /// </summary>
        public ObexConnectPacket() : base(new ObexOpcode(ObexOperation.ServiceUnavailable, true))
        { }

        public ObexConnectPacket(ObexServiceUuid targetService) : this(false, targetService)
        { }

        public ObexConnectPacket(bool disconnect, ObexServiceUuid targetService) : base(new ObexOpcode(disconnect ? ObexOperation.Disconnect : ObexOperation.Connect, true))
        {
            Headers[HeaderId.Target] = new ObexHeader(HeaderId.Target, targetService.Value);
        }

        protected override void WriteExtraField(DataWriter writer)
        {
            writer.WriteByte(OBEXVersion);
            writer.WriteByte(Flags);
            writer.WriteUInt16(MaximumPacketLength);
        }

        protected override async Task<uint> ReadExtraField(DataReader reader)
        {
            uint loaded = await reader.LoadAsync(_EXTRA_FIELD_BITS);
            if (loaded != _EXTRA_FIELD_BITS)
            {
                throw new ObexException("The underlying socket was closed before we were able to read the whole data.");
            }
            OBEXVersion = reader.ReadByte();
            Flags = reader.ReadByte();
            MaximumPacketLength = reader.ReadUInt16();
            return _EXTRA_FIELD_BITS;
        }
    }
}

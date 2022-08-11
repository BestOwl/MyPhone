using System;
using System.Buffers.Binary;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Streams
{
    public class UInt16Interpreter : IBufferContentInterpreter<ushort>
    {
        public ushort GetValue(ReadOnlySpan<byte> buffer)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
    }

    public class StringInterpreter : IBufferContentInterpreter<string>
    {
        public Encoding StringEncoding { get; }
        public bool StringIsNullTerminated { get; }

        public StringInterpreter(Encoding stringEncoding, bool stringIsNullTerminated)
        {
            StringEncoding = stringEncoding;
            StringIsNullTerminated = stringIsNullTerminated;
        }

        public string GetValue(ReadOnlySpan<byte> buffer)
        {
            int len = buffer.Length;
            if (StringIsNullTerminated)
            {
                len -= StringEncoding.GetByteCount("\0");
            }
            return StringEncoding.GetString(buffer.Slice(0, len));
        }
    }
}

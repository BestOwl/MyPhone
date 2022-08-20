using GoodTimeStudio.MyPhone.OBEX.Extensions;
using GoodTimeStudio.MyPhone.OBEX.Streams;
using GoodTimeStudio.MyPhone.OBEX.Utilities;
using System;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class AppParameter
    {
        public byte TagId { get; set; }

        public byte ContentLength { get => (byte)Buffer.Length; }

        public byte TotalLength { get => (byte)(ContentLength + 2 * sizeof(byte)); }

        public byte[] Buffer { get; }

        public AppParameter(byte tagId, byte[] buf) 
        {
            if (buf.Length + 2 * sizeof(byte) > byte.MaxValue)
            {
                throw new InvalidOperationException("Content buffer size too large. Max 126 bytes.");
            }

            TagId = tagId;
            Buffer = buf ?? throw new ArgumentNullException(nameof(buf));
        }

        public AppParameter(byte tagId, byte b) : this(tagId, b.ToBigEndianBytes()) { }

        public AppParameter(byte tagId, ushort value) : this(tagId, value.ToBigEndianBytes()) { }

        public AppParameter(byte tagId, int value) : this(tagId, value.ToBigEndianBytes()) { }

        public R GetValue<I, R>() where I : IBufferContentInterpreter<R>, new()
        {
            I interpreter = new I();
            return GetValue(interpreter);
        }

        public T GetValue<T>(IBufferContentInterpreter<T> interpreter)
        {
            return interpreter.GetValue(Buffer);
        }

        public ushort GetValueAsUInt16()
        {
            return GetValue<UInt16Interpreter, ushort>();
        }

        public override bool Equals(object? obj)
        {
            return obj is AppParameter parameter &&
                   TagId == parameter.TagId &&
                   ByteArrayEqualityComparer.Default.Equals(Buffer, parameter.Buffer);
        }

        public override int GetHashCode()
        {
            int hashCode = -913184727;
            hashCode = hashCode * -1521134295 + TagId.GetHashCode();
            hashCode = hashCode * -1521134295 + ByteArrayEqualityComparer.Default.GetHashCode(Buffer);
            return hashCode;
        }
    }
}

using System.Buffers.Binary;

namespace GoodTimeStudio.MyPhone.OBEX.Extensions
{
    public static class IntExtensions
    {
        public static byte[] ToBigEndianBytes(this int i)
        {
            byte[] ret = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(ret, i);
            return ret;
        }

        public static byte[] ToBigEndianBytes(this ushort us)
        {
            byte[] ret = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(ret, us);
            return ret;
        }

        public static byte[] ToBigEndianBytes(this byte b)
        {
            return new byte[] { BinaryPrimitives.ReverseEndianness(b) };
        }
    }
}

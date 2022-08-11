using System.Collections.Generic;
using System.Linq;

namespace GoodTimeStudio.MyPhone.OBEX.Utilities
{
    public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        private static readonly ByteArrayEqualityComparer s_comparer = new ByteArrayEqualityComparer();
        public static ByteArrayEqualityComparer Default { get => s_comparer; }

        public bool Equals(byte[]? x, byte[]? y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return Enumerable.SequenceEqual(x, y);
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                var result = 0;
                foreach (byte b in obj)
                    result = (result * 31) ^ b;
                return result;
            }
        }
    }
}

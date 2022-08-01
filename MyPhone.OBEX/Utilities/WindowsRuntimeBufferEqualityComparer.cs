using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Utilities
{
    public class WindowsRuntimeBufferEqualityComparer : IEqualityComparer<IBuffer>
    {
        private static readonly WindowsRuntimeBufferEqualityComparer s_comparer = new WindowsRuntimeBufferEqualityComparer();
        public static WindowsRuntimeBufferEqualityComparer Default { get => s_comparer; }

        public bool Equals(IBuffer x, IBuffer y)
        {
            return ByteArrayEqualityComparer.Default.Equals(x.ToArray(), y.ToArray());
        }

        public int GetHashCode(IBuffer obj)
        {
            return ByteArrayEqualityComparer.Default.GetHashCode(obj.ToArray());
        }
    }
}

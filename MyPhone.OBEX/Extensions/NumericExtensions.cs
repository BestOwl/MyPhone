using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Extensions
{
    public static class IntExtensions
    {
        public static IBuffer ToBuffer(this int i)
        {
            using (DataWriter writer = new())
            {
                writer.ByteOrder = ByteOrder.BigEndian;
                writer.WriteInt32(i);
                return writer.DetachBuffer();
            }
        }

        public static IBuffer ToBuffer(this ushort us)
        {
            using (DataWriter writer = new())
            {
                writer.ByteOrder = ByteOrder.BigEndian;
                writer.WriteUInt16(us);
                return writer.DetachBuffer();
            }
        }
    }
}

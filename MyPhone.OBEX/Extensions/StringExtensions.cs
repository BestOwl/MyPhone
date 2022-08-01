using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Extensions
{
    public static class StringExtensions
    {
        public static IBuffer ToBuffer(this string text,
            Windows.Storage.Streams.UnicodeEncoding stringEncoding, bool nullTerminated)
        {
            using (DataWriter writer = new())
            {
                writer.ByteOrder = ByteOrder.BigEndian;
                writer.UnicodeEncoding = stringEncoding;
                writer.WriteString(text);
                if (nullTerminated)
                {
                    writer.WriteByte(0);
                    if (stringEncoding != Windows.Storage.Streams.UnicodeEncoding.Utf8)
                    {
                        writer.WriteByte(0);
                    }
                }
                return writer.DetachBuffer();
            }
        }
    }
}

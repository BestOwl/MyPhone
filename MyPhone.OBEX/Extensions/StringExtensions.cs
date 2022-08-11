using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToBytes(this string text, Encoding stringEncoding, bool nullTerminated)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                writer.Write(stringEncoding.GetBytes(text));
                if (nullTerminated)
                {
                    writer.Write(stringEncoding.GetBytes("\0"));
                }
                writer.Flush();
                return memoryStream.ToArray();
            }
        }
    }
}

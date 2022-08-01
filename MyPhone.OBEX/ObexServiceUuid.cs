using System;

namespace GoodTimeStudio.MyPhone.OBEX
{
    public class ObexServiceUuid
    {

        public static readonly ObexServiceUuid MessageAccess = new ObexServiceUuid(new byte[] { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 });
        public static readonly ObexServiceUuid MessageNotification = new ObexServiceUuid(new byte[] { 0xBB, 0x58, 0x2B, 0x41, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 });
        public static readonly ObexServiceUuid PhonebookAccess = new ObexServiceUuid(new byte[] { 0x79, 0x61, 0x35, 0xf0, 0xf0, 0xc5, 0x11, 0xd8, 0x09, 0x66, 0x08, 0x00, 0x20, 0x0c, 0x9a, 0x66 });

        public byte[] Value { get; private set; }

        public ObexServiceUuid(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Length != 16)
            {
                throw new ArgumentException("Uuid value must be a 16-length byte array");
            }
            Value = value;
        }
    }
}

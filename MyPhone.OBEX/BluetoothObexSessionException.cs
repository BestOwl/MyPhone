using System;

namespace MyPhone.OBEX
{
    public class BluetoothObexSessionException : Exception
    {
        public BluetoothObexSessionException()
        {
        }

        public BluetoothObexSessionException(string message) : base(message)
        {
        }

        public BluetoothObexSessionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

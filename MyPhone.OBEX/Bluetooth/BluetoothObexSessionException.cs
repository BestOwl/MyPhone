using System;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.OBEX.Bluetooth
{
    /// <summary>
    /// Represents errors that occured when establishing an OBEX session connection.
    /// </summary>
    public class BluetoothObexSessionException : Exception
    {
        public new ObexException? InnerException { get; }
        public BluetoothError? BluetoothError { get; }
        public SocketErrorStatus? SocketError { get; }

        public BluetoothObexSessionException()
        {
        }

        public BluetoothObexSessionException(string message) : base(message)
        {
        }

        public BluetoothObexSessionException(
            string message,
            ObexException? innerException = null,
            BluetoothError? bluetoothError = null,
            SocketErrorStatus? socketError = null) : base(message)
        {
            InnerException = innerException;
            BluetoothError = bluetoothError;
            SocketError = socketError;
        }
    }

    /// <summary>
    /// The exception that is thrown when the client tries to connect to a service that
    /// is not supported on the remote Bluetooth device.
    /// </summary>
    public class BluetoothServiceNotSupportedException : BluetoothObexSessionException
    {
        public BluetoothServiceNotSupportedException()
        {
        }

        public BluetoothServiceNotSupportedException(string message) : base(message)
        {
        }

        public BluetoothServiceNotSupportedException(string message, ObexException innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the remote Bluetooth device is not available (e.g. Bluetooth turned off), or it is not in range.
    /// </summary>
    public class BluetoothDeviceNotAvailableException : BluetoothObexSessionException
    {
        public BluetoothDeviceNotAvailableException(string message) : base(message)
        {
        }
    }
}

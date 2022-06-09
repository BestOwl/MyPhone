using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Device
{
    /// <summary>
    /// The exception that is thrown when an attempt to pair a device failed.
    /// </summary>
    public class DevicePairingException : Exception
    {
        public DevicePairingResult PairingResult { get; private set; }

        public DevicePairingException(DevicePairingResult result)
        {
            PairingResult = result;
        }

        public DevicePairingException(DevicePairingResult result, string? message) : base(message)
        {
            PairingResult = result;
        }

        public DevicePairingException(DevicePairingResult result, string? message, Exception? innerException) : base(message, innerException)
        {
            PairingResult = result;
        }
    }
}

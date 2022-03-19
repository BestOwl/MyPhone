using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    /// <summary>
    /// The <see langword="interface"/> for managing phone device
    /// </summary>
    public interface IDeviceService
    {
        DeviceInformation? GetCurrentDevice();

        /// <summary>
        /// Connect to phone device
        /// </summary>
        /// <param name="deviceInformation">The information of the device to be connected</param>
        /// <returns>Whether connected to the device succesfully</returns>
        /// <exception cref="ParingCanceledException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        Task<bool> Connect(DeviceInformation deviceInformation);

        /// <summary>
        /// Create a <see cref="DeviceWatcher"/> to enumerate supported devices
        /// </summary>
        /// <returns></returns>
        DeviceWatcher CreateDeviceWatcher();
    }

    public class ParingCanceledException : ApplicationException
    {
        public DevicePairingResult PairingResult;

        public ParingCanceledException(DevicePairingResult result)
        {
            PairingResult = result;
        }

        public ParingCanceledException(DevicePairingResult result, string? message) : base(message)
        {
            PairingResult = result;
        }

        public ParingCanceledException(DevicePairingResult result, string? message, Exception? innerException) : base(message, innerException)
        {
            PairingResult = result;
        }


    }
}

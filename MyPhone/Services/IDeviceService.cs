using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    /// <summary>
    /// The <see langword="interface"/> for managing phone device
    /// </summary>
    public interface IDeviceService
    {
        /// <summary>
        /// Get the <see cref="DeviceInformation"/> of a previsouly registered device
        /// </summary>
        /// <returns>Previsouly registered device. Return null if there is no device registered previsously</returns>
        Task<DeviceInformation?> GetCurrentDeviceAsync();

        /// <summary>
        /// Connect to phone device
        /// </summary>
        /// <param name="deviceInformation">The information of the device to be connected</param>
        /// <returns>Whether connected to the device succesfully</returns>
        /// <exception cref="ParingCanceledException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        Task<bool> ConnectAsync(DeviceInformation deviceInformation);

        /// <summary>
        /// Reconnect to a previsously registered phone device
        /// </summary>
        /// <returns>True if reconnect successfully</returns>
        /// <exception cref="InvalidOperationException">Throws if there is not device registered previously.</exception>
        Task<bool> ReconnectAsync();

        /// <summary>
        /// Create a <see cref="DeviceWatcher"/> to enumerate supported devices
        /// </summary>
        /// <returns></returns>
        DeviceWatcher CreateDeviceWatcher();

        /// <summary>
        /// Create a <see cref="PhoneLineWatcher"/> to enumerate available phone lines
        /// </summary>
        /// <returns></returns>
        Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync();

        /// <summary>
        /// Call the given phone number via the given phone line.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <exception cref="OperationCanceledException">Thorws if calling request timeout or the phone line is currently not available.</exception>
        Task CallAsync(string phoneNumber);
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

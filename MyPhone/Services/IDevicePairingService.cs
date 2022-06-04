using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    /// <summary>
    /// The <see langword="interface"/> for managing phone device
    /// </summary>
    public interface IDevicePairingService
    {
        /// <summary>
        /// Create a <see cref="DeviceWatcher"/> to enumerate supported devices' <see cref="DeviceInformation"/>
        /// </summary>
        /// <returns></returns>
        DeviceWatcher CreateDeviceWatcher();

        /// <summary>
        /// Check whether the given Bluetooth device is paired
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> of a device</param>
        /// <returns>True if the device is paired, false otherwise.</returns>
        bool IsPaired(DeviceInformation deviceInformation);

        /// <summary>
        /// Pair the device.
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> of a device.</param>
        /// <returns>The pair result</returns>
        Task<DevicePairingResult> PairDeviceAsync(DeviceInformation deviceInformation);
    }
}

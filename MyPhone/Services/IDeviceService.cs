using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    /// <summary>
    /// The <see langword="interface"/> for managing phone device
    /// </summary>
    public interface IDeviceService
    {
        /// <summary>
        /// Get the <see cref="CurrentDeviceInformation"/> of a current registered device
        /// </summary>
        /// <returns>Current registered device. Return null if there is no device registered</returns>
        Task<CurrentDeviceInformation?> GetCurrentRegisteredDeviceAsync();

        /// <summary>
        /// Create a <see cref="DeviceWatcher"/> to enumerate supported devices' <see cref="DeviceInformation"/>
        /// </summary>
        /// <returns></returns>
        DeviceWatcher CreateDeviceWatcher();

        /// <summary>
        /// Pair the Bluetooth device with its <see cref="DeviceInformation"/>.
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> of a <see cref="BluetoothDevice"/>.</param>
        /// <returns>The pair result</returns>
        Task<DevicePairingResult> PairDeviceAsync(DeviceInformation deviceInformation);

        /// <summary>
        /// Register a Bluetooth device with its <see cref="DeviceInformation"/>
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> of a <see cref="BluetoothDevice"/>. This device must be paired.</param>
        /// <returns>The newly registered <see cref="CurrentDeviceInformation"/>. Returns null if failed</returns>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        Task<CurrentDeviceInformation?> RegisterDeviceAsync(DeviceInformation deviceInformation);

        /// <summary>
        /// Create a <see cref="PhoneLineWatcher"/> to enumerate available phone lines
        /// </summary>
        /// <returns></returns>
        Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync();
    }

    public class CurrentDeviceInformation
    {
        public BluetoothDevice BluetoothDevice { get; private set; }
        public PhoneLineTransportDevice PhoneLineTransportDevice { get; private set; }

        public CurrentDeviceInformation(BluetoothDevice bluetoothDevice, PhoneLineTransportDevice phoneLineTransportDevice)
        {
            BluetoothDevice = bluetoothDevice ?? throw new ArgumentNullException(nameof(bluetoothDevice));
            PhoneLineTransportDevice = phoneLineTransportDevice ?? throw new ArgumentNullException(nameof(phoneLineTransportDevice));
        }
    }
}

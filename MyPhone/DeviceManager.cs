using GoodTimeStudio.MyPhone.Helpers;
using GoodTimeStudio.MyPhone.Services;
using GoodTimeStudio.MyPhone.Utilities;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone
{
    public class DeviceManager : IDisposable
    {
        private readonly IDevicePairingService _deviceService;
        private readonly ISettingsService _settingsService;

        private bool _inited;
        private DeviceInformation? _currentDeviceInformation;
        private BluetoothDevice? _currentDevice;
        
        public DeviceCallServiceProvider? CallService { get; private set; }
        public DeviceSmsServiceProvider? SmsService { get; private set; }

        public DeviceManager(IDevicePairingService deviceService, ISettingsService settingsService)
        {
            _inited = false;
            _deviceService = deviceService;
            _settingsService = settingsService;
        }

        /// <summary>
        /// Attempt to reconnect to a previously registered device.
        /// </summary>
        /// <returns>Returns true if a previously registered device is found, false otherwise.</returns>
        public async Task<bool> TryReconnectAsync()
        {
            if (_inited)
            {
                throw new InvalidOperationException("DeviceManger is already connected to a device");
            }

            string? deviceId = _settingsService.GetValue<string>(_settingsService.KeyCurrentDeviceId);
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            DeviceInformation deviceInformation = await DeviceInformation.CreateFromIdAsync(deviceId);
            if (!_deviceService.IsPaired(deviceInformation))
            {
                return false;
            }

            await InitializeDevice(deviceInformation);
            _ = ConnectAllDeviceServices();
            _inited = true;
            return true;
        }

        /// <summary>
        /// Connect and register the new device.
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> object obtained from <see cref="Windows.Devices.Enumeration"/> API</param>
        /// <returns>Whether connected to the device succesfully</returns>
        /// <remarks>
        /// The <see cref="DeviceInformation.Kind"/> must be <see cref="DeviceInformationKind.AssociationEndpoint"/> and it must be a Bluetooth device
        /// </remarks>
        /// <exception cref="DeviceParingException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        public async Task<bool> ConnectAsync(DeviceInformation deviceInformation)
        {
            if (_inited)
            {
                throw new InvalidOperationException("DeviceManger is already connected to a device");
            }
            if (deviceInformation == null)
            {
                throw new ArgumentNullException(nameof(deviceInformation));
            }
            if (deviceInformation.Kind != DeviceInformationKind.AssociationEndpoint)
            {
                throw new InvalidOperationException("Does not support this device");
            }

            if (!_deviceService.IsPaired(deviceInformation))
            {
                var paringResult = await _deviceService.PairDeviceAsync(deviceInformation);
                if (paringResult.Status != DevicePairingResultStatus.Paired)
                {
                    throw new DeviceParingException(paringResult);
                }
            }

            _settingsService.SetValue(_settingsService.KeyCurrentDeviceId, deviceInformation.Id);

            await InitializeDevice(deviceInformation);
            await ConnectAllDeviceServices();
            _inited = true;
            return true;
        }

        private async Task InitializeDevice(DeviceInformation deviceInformation)
        {
            _currentDeviceInformation = deviceInformation;
            _currentDevice = await BluetoothDevice.FromIdAsync(deviceInformation.Id);

            PhoneLineTransportDevice? phoneLineTransportDevice = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(_currentDevice);
            if (phoneLineTransportDevice != null)
            {
                DeviceAccessStatus accessStatus = await phoneLineTransportDevice.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    CallService = new DeviceCallServiceProvider(_currentDevice, phoneLineTransportDevice);
                }
            }
            SmsService = new DeviceSmsServiceProvider(_currentDevice);
        }

        /// <summary>
        /// Connect to all device services.
        /// </summary>
        /// <returns>Whether all device service is connected successfully</returns>
        /// <exception cref="InvalidOperationException">Throws if the client call this method in an invalid state</exception>
        private async Task<bool> ConnectAllDeviceServices()
        {
            if (_currentDevice == null)
            {
                throw new InvalidOperationException("You must first call ConnectAsync or TryReconnect (return true).");
            }

            bool connected = true;
            if (CallService != null)
            {
                connected = connected && await CallService.ConnectAsync();
            }
            connected = connected && await SmsService!.ConnectAsync();
            return connected;
        }

        public void Dispose()
        {
            CallService?.Dispose();
            SmsService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class DeviceParingException : Exception
    {
        public DevicePairingResult PairingResult { get; private set; }

        public DeviceParingException(DevicePairingResult result)
        {
            PairingResult = result;
        }

        public DeviceParingException(DevicePairingResult result, string? message) : base(message)
        {
            PairingResult = result;
        }

        public DeviceParingException(DevicePairingResult result, string? message, Exception? innerException) : base(message, innerException)
        {
            PairingResult = result;
        }
    }
}

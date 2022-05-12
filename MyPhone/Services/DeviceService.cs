using GoodTimeStudio.MyPhone.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDevicePairDialogService _devicePairDialogService;
        private readonly ISettingsService _settingsService;

        public DeviceService(IDevicePairDialogService devicePairDialogService, ISettingsService settingsService)
        {
            _devicePairDialogService = devicePairDialogService;
            _settingsService = settingsService;
        }

        public async Task<CurrentDeviceInformation?> GetCurrentRegisteredDeviceAsync()
        {
            string? btdId = _settingsService.GetValue<string>(_settingsService.KeyCurrentBluetoothDeviceId);
            if (btdId == null)
            {
                return null;
            }
            string? pltdId = _settingsService.GetValue<string>(_settingsService.KeyCurrentPhoneLineTransportDeviceId);
            if (pltdId == null)
            {
                return null;
            }

            BluetoothDevice btd = await BluetoothDevice.FromIdAsync(btdId);
            PhoneLineTransportDevice pltd = PhoneLineTransportDevice.FromId(pltdId);
            return new CurrentDeviceInformation(btd, pltd);
        }

        public async Task<DevicePairingResult> PairDeviceAsync(DeviceInformation deviceInformation)
        {
            if (deviceInformation == null)
            {
                throw new ArgumentNullException(nameof(deviceInformation));
            }
            if (deviceInformation.Kind != DeviceInformationKind.AssociationEndpoint)
            {
                throw new InvalidOperationException("Does not support this device");
            }

            deviceInformation.Pairing.Custom.PairingRequested += Custom_PairingRequested;
            DevicePairingResult pairingResult = await deviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmPinMatch);
            deviceInformation.Pairing.Custom.PairingRequested -= Custom_PairingRequested;
            _devicePairDialogService.HideDialog();
            return pairingResult;
        }

        private async void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            bool accept = await _devicePairDialogService.ShowPairDialogAsync(args.DeviceInformation.Name, args.Pin, TimeSpan.FromSeconds(30));
            if (accept)
            {
                args.Accept();
            }
            deferral.Complete();
        }

        public async Task<CurrentDeviceInformation?> RegisterDeviceAsync(DeviceInformation deviceInformation)
        {
            BluetoothDevice bt = await BluetoothDevice.FromIdAsync(deviceInformation.Id);
            PhoneLineTransportDevice? pltd = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(bt);
            if (pltd == null)
            {
                return null;
            }
            await pltd.RequestAccessAsync();
            pltd.RegisterApp();

            if (await pltd.ConnectAsync())
            {
                _settingsService.SetValue(_settingsService.KeyCurrentBluetoothDeviceId, bt.DeviceId);
                _settingsService.SetValue(_settingsService.KeyCurrentPhoneLineTransportDeviceId, pltd.DeviceId);
                return new CurrentDeviceInformation(bt, pltd);
            }
            else
            {
                pltd.UnregisterApp();
                return null;
            }
        }

        public async Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync()
        {
            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
            PhoneLineWatcher watcher = store.RequestLineWatcher();
            return watcher;
        }

        public DeviceWatcher CreateDeviceWatcher()
        {
            // Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            // So we need to do it mannually, this is the Bluetooth selector that include both paired and unpaird devices
            string bluttoothSelector = "System.Devices.Aep.ProtocolId:=\"{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}\" AND (System.Devices.Aep.CanPair:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True)";
            return DeviceInformation.CreateWatcher(bluttoothSelector, null, DeviceInformationKind.AssociationEndpoint);
        }
    }
}

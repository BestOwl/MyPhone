using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    public class DevicePairingService : IDevicePairingService
    {
        private readonly IDevicePairDialogService _devicePairDialogService;

        public DevicePairingService(IDevicePairDialogService devicePairDialogService)
        {
            _devicePairDialogService = devicePairDialogService;
        }

        public bool IsPaired(DeviceInformation deviceInformation)
        {
            return deviceInformation.Pairing.IsPaired;
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

        public DeviceWatcher CreateDeviceWatcher()
        {
            // Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            // So we need to do it mannually, this is the Bluetooth selector that include both paired and unpaird devices
            string bluttoothSelector = "System.Devices.Aep.ProtocolId:=\"{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}\" AND (System.Devices.Aep.CanPair:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True)";
            return DeviceInformation.CreateWatcher(bluttoothSelector, null, DeviceInformationKind.AssociationEndpoint);
        }
    }

    public static class BluetoothDeviceServiceExtensions
    {
        public static IServiceCollection AddDevicePairingService(this IServiceCollection services)
        {
            services.AddTransient<IDevicePairingService, DevicePairingService>();
            return services;
        }
    }
}

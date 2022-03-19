using GoodTimeStudio.MyPhone.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDevicePairDialogService devicePairDialogService;

        public DeviceService(IDevicePairDialogService devicePairDialogService)
        {
            this.devicePairDialogService = devicePairDialogService;
        }

        /// <summary>
        /// Connect to phone device
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> object obtained from <see cref="Windows.Devices.Enumeration"/> API</param>
        /// <returns>Whether connected to the device succesfully</returns>
        /// <remarks>
        /// The <see cref="DeviceInformation.Kind"/> must be <see cref="DeviceInformationKind.AssociationEndpoint"/> and it must be a Bluetooth device
        /// </remarks>
        /// <exception cref="ParingCanceledException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        public async Task<bool> Connect(DeviceInformation deviceInformation)
        {
            if (deviceInformation == null)
            {
                throw new ArgumentNullException(nameof(deviceInformation));
            }
            if (deviceInformation.Kind != DeviceInformationKind.AssociationEndpoint)
            {
                throw new InvalidOperationException("Does not support this device");
            }

            if (!deviceInformation.Pairing.IsPaired)
            {
                if (deviceInformation.Pairing.CanPair)
                {
                    deviceInformation.Pairing.Custom.PairingRequested += Custom_PairingRequested;
                    DevicePairingResult pairingResult = await deviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmPinMatch);
                    System.Diagnostics.Debug.WriteLine("PairAsync return");
                    deviceInformation.Pairing.Custom.PairingRequested -= Custom_PairingRequested;
                    devicePairDialogService.HideDialog();
                    if (pairingResult.Status != DevicePairingResultStatus.Paired)
                    {
                        System.Diagnostics.Debug.WriteLine("Pair failed");
                        throw new ParingCanceledException(pairingResult);
                    }
                    System.Diagnostics.Debug.WriteLine("Pair success");
                }
                else
                {
                    return false;
                }
            }

            BluetoothDevice bt = await BluetoothDevice.FromIdAsync(deviceInformation.Id);
            PhoneLineTransportDevice? pltd = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(bt);
            if (pltd == null)
            {
                return false;
            }
            await pltd.RequestAccessAsync();
            pltd.RegisterApp();
            
            if (await pltd.ConnectAsync())
            {
                return true;
            }
            else
            {
                pltd.UnregisterApp();
                return false;
            }
        }

        private async void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            MainWindow.Instance.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                bool accept = await devicePairDialogService.ShowPairDialogAsync(args.DeviceInformation.Name, args.Pin, TimeSpan.FromSeconds(30));
                taskCompletionSource.SetResult(accept);
            });

            if (await taskCompletionSource.Task)
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

        public DeviceInformation? GetCurrentDevice()
        {
            throw new NotImplementedException();
        }

 
    }

    
}

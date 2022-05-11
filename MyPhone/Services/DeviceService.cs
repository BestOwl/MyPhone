using GoodTimeStudio.MyPhone.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDevicePairDialogService devicePairDialogService;
        private readonly ISettingsService settingsService;

        private CurrentDeviceInformation? currentDevice;

        private Task taskInitPhoneLine;
        private PhoneLine? selectedPhoneLine;

        public DeviceService(IDevicePairDialogService devicePairDialogService, ISettingsService settingsService)
        {
            this.devicePairDialogService = devicePairDialogService;
            this.settingsService = settingsService;
            taskInitPhoneLine = InitPhoneLine();
        }

        #region Init PhoneLine
        // https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.calls.phonelinewatcher?view=winrt-22000
        private async Task InitPhoneLine()
        {
            if (currentDevice == null)
            {
                await GetCurrentDeviceAsync();
            }
            // Do NOT use 'else' here because GetCurrentDeviceAsync() may update currentDevice,
            // we must check currentDevice is not null again
            if (currentDevice != null)
            {

                List<PhoneLine> phoneLinesAvailable = new List<PhoneLine>();
                var lineEnumerationCompletion = new TaskCompletionSource<bool>();

                PhoneLineWatcher phoneLineWatcher = await CreatePhoneLineWatcherAsync();
                phoneLineWatcher.LineAdded += async (o, args) =>
                {
                    phoneLinesAvailable.Add(await PhoneLine.FromIdAsync(args.LineId));
                };
                phoneLineWatcher.Stopped += (o, args) => lineEnumerationCompletion.TrySetResult(false);
                phoneLineWatcher.EnumerationCompleted += (o, args) => lineEnumerationCompletion.TrySetResult(true);
                phoneLineWatcher.Start();

                // Wait for enumeration completion
                if (await lineEnumerationCompletion.Task)
                {
                    selectedPhoneLine = phoneLinesAvailable
                        .Where(pl => pl.TransportDeviceId == currentDevice!.PhoneLineTransportDevice.DeviceId)
                        .FirstOrDefault();
                }

                phoneLineWatcher.Stop();
                System.Diagnostics.Debug.WriteLine("PhoneLineWatcher stopped");
            }

        }
        #endregion Init PhoneLine

        /// <summary>
        /// Connect to a new phone device and register it
        /// </summary>
        /// <param name="deviceInformation">The <see cref="DeviceInformation"/> object obtained from <see cref="Windows.Devices.Enumeration"/> API</param>
        /// <returns>Whether connected to the device succesfully</returns>
        /// <remarks>
        /// The <see cref="DeviceInformation.Kind"/> must be <see cref="DeviceInformationKind.AssociationEndpoint"/> and it must be a Bluetooth device
        /// </remarks>
        /// <exception cref="ParingCanceledException">Throws when the user cancel the pairing, or the pairing failed because of other reasons</exception>
        /// <exception cref="UnauthorizedAccessException">Throws when the operating system denied the access to the device</exception>
        public async Task<bool> ConnectAsync(DeviceInformation deviceInformation)
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
                settingsService.SetValue(settingsService.KeyCurrentBluetoothDeviceId, bt.DeviceId);
                settingsService.SetValue(settingsService.KeyCurrentPhoneLineTransportDeviceId, pltd.DeviceId);
                currentDevice = new CurrentDeviceInformation(bt, pltd);
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

        public async Task<DeviceInformation?> GetCurrentDeviceAsync()
        {
            if (currentDevice != null)
            {
                return currentDevice.BluetoothDevice.DeviceInformation;
            }
            else
            {
                string? btdId = settingsService.GetValue<string>(settingsService.KeyCurrentBluetoothDeviceId);
                if (btdId == null)
                {
                    return null;
                }
                string? pltdId = settingsService.GetValue<string>(settingsService.KeyCurrentPhoneLineTransportDeviceId);
                if (pltdId == null)
                {
                    return null;
                }

                BluetoothDevice btd = await BluetoothDevice.FromIdAsync(btdId);
                PhoneLineTransportDevice pltd = PhoneLineTransportDevice.FromId(pltdId);
                currentDevice = new CurrentDeviceInformation(btd, pltd);
                return btd.DeviceInformation;
            }
        }

        public async Task<bool> ReconnectAsync()
        {
            // Ensure currentDevice not null
            await GetCurrentDeviceAsync();

            if (currentDevice == null)
            {
                throw new InvalidOperationException("Not device registered previously");
            }

            await currentDevice.PhoneLineTransportDevice.RequestAccessAsync();
            if (!currentDevice.PhoneLineTransportDevice.IsRegistered())
            {
                currentDevice.PhoneLineTransportDevice.RegisterApp();
            }
            return await currentDevice.PhoneLineTransportDevice.ConnectAsync();
        }

        public async Task CallAsync(string phoneNumber)
        {
            await taskInitPhoneLine;
            if (selectedPhoneLine == null || !selectedPhoneLine.CanDial)
            {
                throw new OperationCanceledException();
            }
            // TODO: Lookup contacts book for displayName
            selectedPhoneLine.Dial(phoneNumber, phoneNumber);
        }

        public async Task<PhoneLineWatcher> CreatePhoneLineWatcherAsync()
        {
            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
            PhoneLineWatcher watcher = store.RequestLineWatcher();
            return watcher;
        }
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

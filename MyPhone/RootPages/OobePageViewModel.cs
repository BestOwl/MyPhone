using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoodTimeStudio.MyPhone.Device;
using GoodTimeStudio.MyPhone.Models;
using GoodTimeStudio.MyPhone.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace GoodTimeStudio.MyPhone.RootPages
{
    public partial class OobePageViewModel : ObservableObject
    {
        public OobePageViewModel()
        {
            DeviceConnectCommand = new AsyncRelayCommand(Connect);
        }

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(EnableConnectButton))]
        private ObservableDeviceInformation? selectedDevice;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(EnableConnectButton))]
        private bool connecting;

        public bool EnableConnectButton
        {
            get => !connecting && selectedDevice != null;
        }

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(ErrorTextVisbility))]
        private string? errorText;

        public bool ErrorTextVisbility
        {
            get => errorText != null;
        }

        public IAsyncRelayCommand DeviceConnectCommand { get; }

        public event EventHandler? OobeCompletedEvent;

        public async Task Connect()
        {
            Debug.Assert(selectedDevice != null);

            Connecting = true;
            ErrorText = null;
            try
            {
                await App.Current.SetupDevice(selectedDevice.DeviceInformation, true);
                OobeCompletedEvent?.Invoke(this, new EventArgs());
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorText = "Sorry, but we do not have the permission to connect your phone. " + ex.Message;
            }
            catch (DevicePairingException ex)
            {
                ErrorText = "Failed to pair \"" + selectedDevice!.Name + " \". Reason: "
                       + ex.PairingResult.Status.ToString();
            }

            Connecting = false;
        }


    }

}

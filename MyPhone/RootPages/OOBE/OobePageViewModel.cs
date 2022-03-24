using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoodTimeStudio.MyPhone.Models;
using GoodTimeStudio.MyPhone.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GoodTimeStudio.MyPhone.RootPages.OOBE
{
    public partial class OobePageViewModel : ObservableObject
    {
        public OobePageViewModel(IDeviceService deviceService, ISettingsService settingsService)
        {
            this.deviceService = deviceService;
            this.settingsService = settingsService;
            DeviceConnectCommand = new AsyncRelayCommand(Connect);
        }

        private readonly IDeviceService deviceService;
        private readonly ISettingsService settingsService;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(EnableConnectButton))]
        private DeviceInformationEx? selectedDevice;

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
        
        public IAsyncRelayCommand DeviceConnectCommand;

        public event EventHandler? OobeCompletedEvent;

        public async Task<bool> Connect()
        {
            Connecting = true;
            ErrorText = null;
            try
            {
                if (await deviceService.ConnectAsync(selectedDevice!.DeviceInformation))
                {
                    settingsService.SetValue(settingsService.KeyOobeIsCompleted, true);
                    OobeCompletedEvent?.Invoke(this, new EventArgs());
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorText = "Sorry, but we do not have the permission to connect your phone. " + ex.Message;
            }
            catch (ParingCanceledException ex)
            {
                ErrorText = "Failed to pair \"" + selectedDevice!.Name + " \". Reason: "
                       + ex.PairingResult.Status.ToString();
            }

            Connecting = false;
            return true;
        }


    }

}

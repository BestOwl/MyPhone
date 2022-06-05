using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoodTimeStudio.MyPhone.Models;
using GoodTimeStudio.MyPhone.Services;
using System;
using System.Threading.Tasks;


namespace GoodTimeStudio.MyPhone.RootPages
{
    public partial class OobePageViewModel : ObservableObject
    {
        public OobePageViewModel(DeviceManager deviceManager, ISettingsService settingsService)
        {
            _deviceManager = deviceManager;
            _settingsService = settingsService;
            DeviceConnectCommand = new AsyncRelayCommand(Connect);
        }

        private readonly DeviceManager _deviceManager;
        private readonly ISettingsService _settingsService;

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

        public async Task Connect()
        {
            Connecting = true;
            ErrorText = null;
            try
            {
                if (await _deviceManager.ConnectAsync(selectedDevice!.DeviceInformation))
                {
                    _settingsService.SetValue(_settingsService.KeyOobeIsCompleted, true);
                    OobeCompletedEvent?.Invoke(this, new EventArgs());
                }
                else
                {
                    ErrorText = "Unable to connect to your phone, please try again.";
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorText = "Sorry, but we do not have the permission to connect your phone. " + ex.Message;
            }
            catch (DeviceParingException ex)
            {
                ErrorText = "Failed to pair \"" + selectedDevice!.Name + " \". Reason: "
                       + ex.PairingResult.Status.ToString();
            }

            Connecting = false;
        }


    }

}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using GoodTimeStudio.MyPhone.Device;
using GoodTimeStudio.MyPhone.Models;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace GoodTimeStudio.MyPhone.RootPages
{
    public partial class OobePageViewModel : ObservableObject
    {
        private readonly ILogger<OobePageViewModel> _logger;

        public OobePageViewModel(ILogger<OobePageViewModel> logger)
        {
            DeviceConnectCommand = new AsyncRelayCommand(Connect);
            _logger = logger;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableConnectButton))]
        private ObservableDeviceInformation? selectedDevice;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableConnectButton))]
        private bool connecting;

        public bool EnableConnectButton
        {
            get => !connecting && selectedDevice != null;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ErrorTextVisbility))]
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
            ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
            try
            {
                await App.Current.SetupDevice(selectedDevice.DeviceInformation, true);
                OobeCompletedEvent?.Invoke(this, new EventArgs());
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(AppLogEvents.DeviceSetup, ex, "Failed to setup this device.");
                ErrorText = resourceLoader.GetString("Permission_Error") + ex.Message;
            }
            catch (DevicePairingException ex)
            {
                _logger.LogWarning(AppLogEvents.DeviceSetup, ex, "Failed to setup this device.");
                ErrorText = resourceLoader.GetString("Failed_Connect_Error_Front") + selectedDevice!.Name + resourceLoader.GetString("Failed_Connect_Error_Second")
                       + ex.PairingResult.Status.ToString();
            }

            Connecting = false;
        }


    }

}

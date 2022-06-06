using System;
using System.ComponentModel;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;
using Windows.UI;

namespace GoodTimeStudio.MyPhone.Models
{
    public class ObservablePhoneLine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public PhoneLine PhoneLine { get; private set; }

        public bool CanDial => PhoneLine.CanDial;
        public PhoneCallVideoCapabilities VideoCallingCapabilities => PhoneLine.VideoCallingCapabilities;
        public string TransportDeviceId => PhoneLine.TransportDeviceId;
        public PhoneLineTransport Transport => PhoneLine.Transport;
        public bool SupportsTile => PhoneLine.SupportsTile;
        public PhoneNetworkState NetworkState => PhoneLine.NetworkState;
        public string NetworkName => PhoneLine.NetworkName;
        public PhoneLineConfiguration LineConfiguration => PhoneLine.LineConfiguration;
        public Guid Id => PhoneLine.Id;
        public string DisplayName => PhoneLine.DisplayName;
        public Color DisplayColor => PhoneLine.DisplayColor;
        public PhoneLineCellularDetails CellularDetails => PhoneLine.CellularDetails;
        public PhoneVoicemail Voicemail => PhoneLine.Voicemail;

        public string? TransportDeviceName { get; private set; }


        public ObservablePhoneLine(PhoneLine phoneLine)
        {
            PhoneLine = phoneLine;
            PhoneLine.LineChanged += PhoneLine_LineChanged;
            UpdateTransportDeviceName();
        }

        private void PhoneLine_LineChanged(PhoneLine sender, object args)
        {
            PhoneLine = sender;
            OnPropertyChanged(nameof(CanDial));
            OnPropertyChanged(nameof(VideoCallingCapabilities));
            OnPropertyChanged(nameof(TransportDeviceId));
            OnPropertyChanged(nameof(SupportsTile));
            OnPropertyChanged(nameof(NetworkState));
            OnPropertyChanged(nameof(NetworkName));
            OnPropertyChanged(nameof(LineConfiguration));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(DisplayColor));
            OnPropertyChanged(nameof(CellularDetails));
            OnPropertyChanged(nameof(Voicemail));
        }

        private async void UpdateTransportDeviceName()
        {
            DeviceInformation information = await DeviceInformation.CreateFromIdAsync(TransportDeviceId);
            TransportDeviceName = information.Name;
            OnPropertyChanged(nameof(TransportDeviceName));
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}

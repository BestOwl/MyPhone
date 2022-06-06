using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Models
{
    public class ObservableDeviceInformation : INotifyPropertyChanged
    {
        public ObservableDeviceInformation(DeviceInformation deviceInfoIn)
        {
            DeviceInformation = deviceInfoIn;
            UpdateThumbnailBitmapImage();
        }

        public DeviceInformationKind Kind => DeviceInformation.Kind;
        public string Id => DeviceInformation.Id;
        public string Name => DeviceInformation.Name;
        public BitmapImage? ThumbnailBitmapImage { get; private set; }
        public bool CanPair => DeviceInformation.Pairing.CanPair;
        public bool IsPaired => DeviceInformation.Pairing.IsPaired;
        public IReadOnlyDictionary<string, object> Properties => DeviceInformation.Properties;
        public DeviceInformation DeviceInformation { get; private set; }

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation.Update(deviceInfoUpdate);

            OnPropertyChanged(nameof(Kind));
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DeviceInformation));
            OnPropertyChanged(nameof(CanPair));
            OnPropertyChanged(nameof(IsPaired));
            OnPropertyChanged(nameof(GetPropertyForDisplay));

            UpdateThumbnailBitmapImage();
        }

        public string? GetPropertyForDisplay(string key) => Properties[key]?.ToString();

        private async void UpdateThumbnailBitmapImage()
        {
            DeviceThumbnail deviceThumbnail = await DeviceInformation.GetThumbnailAsync();
            BitmapImage glyphBitmapImage = new BitmapImage();
            await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            ThumbnailBitmapImage = glyphBitmapImage;
            OnPropertyChanged(nameof(ThumbnailBitmapImage));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

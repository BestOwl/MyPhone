using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Models
{
    public class DeviceInformationEx : INotifyPropertyChanged
    {
        public DeviceInformationEx(DeviceInformation deviceInfoIn)
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

            OnPropertyChanged("Kind");
            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("CanPair");
            OnPropertyChanged("IsPaired");
            OnPropertyChanged("GetPropertyForDisplay");

            UpdateThumbnailBitmapImage();
        }

        public string? GetPropertyForDisplay(string key) => Properties[key]?.ToString();

        private async void UpdateThumbnailBitmapImage()
        {
            DeviceThumbnail deviceThumbnail = await DeviceInformation.GetThumbnailAsync();
            BitmapImage glyphBitmapImage = new BitmapImage();
            await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            ThumbnailBitmapImage = glyphBitmapImage;
            OnPropertyChanged("ThumbnailBitmapImage");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

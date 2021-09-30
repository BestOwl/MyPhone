using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.ViewModels
{
    public class TestPageViewModel : BindableBase
    {
        public ObservableCollection<Guid> PhoneLines = new ObservableCollection<Guid>();
        public ObservableCollection<DeviceInformation> PLTDevices = new ObservableCollection<DeviceInformation>();

        private DeviceInformation _SeletedDevice;
        public DeviceInformation SelectedDevice
        {
            get => _SeletedDevice;
            set => SetProperty(ref _SeletedDevice, value);
        }

        private Guid _SelectedPhoneLine;
        public Guid SelectedPhoneLine
        {
            get => _SelectedPhoneLine;
            set => SetProperty(ref _SelectedPhoneLine, value);
        }

        private string _RegistrationStatus;
        public string RegistrationStatus
        {
            get => _RegistrationStatus;
            set => SetProperty(ref _RegistrationStatus, "Registration status: " + value);
        }

        private string _ConnectionStatus;
        public string ConnectionStatus
        {
            get => _ConnectionStatus;
            set => SetProperty(ref _ConnectionStatus, "Connection status: " + value);
        }

        private bool _IsWorking;
        public bool IsWorking
        {
            get => _IsWorking;
            set 
            { 
                SetProperty(ref _IsWorking, value);
                OnPropertyChanged("IsNotWorking");
            }
        }

        public bool IsNotWorking
        {
            get => !IsWorking;
        }
    }
}

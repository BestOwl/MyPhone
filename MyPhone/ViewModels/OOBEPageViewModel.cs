using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GoodTimeStudio.MyPhone.ViewModels
{
    public class OOBEPageViewModel : BindableBase
    {
        public BluetoothDeviceListViewModel DListModel;

        public event EventHandler OOBECompletedEvent;

        private bool _IsWorking;
        public bool IsWorking
        {
            get => _IsWorking;
            set 
            {
                SetProperty(ref _IsWorking, value);
                OnPropertyChanged(nameof(IsNotWorking));
            }
        }

        public bool IsNotWorking
        {
            get => !IsWorking;
        }

        public OOBEPageViewModel(BluetoothDeviceListViewModel listModel)
        {
            DListModel = listModel;
        }

        public async Task Connect()
        {
            IsWorking = true;

            if (await DeviceManager.ConnectTo(DListModel.SelectedDevice))
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings["OOBE"] = false;
                App.Navigate(typeof(MainPage));
            }
            else
            {
                //TODO: Notify user fail to connect
            }

            IsWorking = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GoodTimeStudio.MyPhone.Models
{
    public class OOBEPageViewModel : BindableBase
    {
        public BluetoothDeviceListViewModel DListModel;

        public event EventHandler OOBECompletedEvent;

        public OOBEPageViewModel(BluetoothDeviceListViewModel listModel)
        {
            DListModel = listModel;
        }

        public async Task Connect()
        {
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
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml;

namespace GoodTimeStudio.MyPhone.Pages.Call
{
    public partial class CallPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string phoneNumber;

        [ObservableProperty]
        private int inputIndex;

        [ObservableProperty]
        private int selectionLength;

        //public void ButtonCall_Click(object sender, RoutedEventArgs e)
        //{
        //    DeviceManager.Call(PhoneNumber);
        //}
        
    }
}

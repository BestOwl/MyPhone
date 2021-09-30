using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml;

namespace GoodTimeStudio.MyPhone.ViewModels
{
    public class CallPageViewModel : BindableBase
    {
        private string _PhoneNumber;
        public string PhoneNumber
        {
            get => _PhoneNumber;
            set => SetProperty(ref _PhoneNumber, value);
        }

        private int _InputIndex;
        public int InputIndex
        {
            get => _InputIndex;
            set => SetProperty(ref _InputIndex, value);
        }

        private int _SelectionLength;
        public int SelectionLength
        {
            get => _SelectionLength;
            set => SetProperty(ref _SelectionLength, value);
        }

        public void ButtonCall_Click(object sender, RoutedEventArgs e)
        {
            DeviceManager.Call(PhoneNumber);
        }
        
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Models
{
    public partial class DeviceServiceProviderInformation : ObservableObject
    {
        [ObservableProperty]
        private DeviceServiceProviderState _state;

        [ObservableProperty]
        private string? _statusMessage;
    }
}

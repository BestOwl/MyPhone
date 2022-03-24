using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.Mvvm.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GoodTimeStudio.MyPhone.Pages.Call
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CallPage : Page
    {
        public CallPageViewModel ViewModel => (CallPageViewModel)DataContext;

        public CallPage()
        {
            this.InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<CallPageViewModel>();
            ViewModel.PhoneNumberInputFocus += ViewModel_PhoneNumberInputFocus;
        }

        private void ViewModel_PhoneNumberInputFocus(object? sender, EventArgs e)
        {
            PhoneNumInput.Focus(FocusState.Pointer);
        }
    }
}

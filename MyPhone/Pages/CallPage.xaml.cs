using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GoodTimeStudio.MyPhone.Pages
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

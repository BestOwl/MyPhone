using GoodTimeStudio.MyPhone.Pages.Call;
using GoodTimeStudio.MyPhone.Pages.Diagnosis;
using GoodTimeStudio.MyPhone.Pages.Message;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone.RootPages.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel => (MainPageViewModel)DataContext;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = new MainPageViewModel();
        }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {

            }
            else
            {
                NavigationMenu item = (NavigationMenu)args.SelectedItem; //TODO: better MVVM
                switch (item.Name)
                {
                    case "Call":
                        contentFrame.Navigate(typeof(CallPage));
                        break;
                    case "Message":
                        contentFrame.Navigate(typeof(MessagePage));
                        break;
                    case "Debug":
                        contentFrame.Navigate(typeof(DiagnosisPage));
                        break;
                }
            }
        }
    }
}

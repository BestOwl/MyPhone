using GoodTimeStudio.MyPhone.Pages;
using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone.RootPages
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
            NavigationMenu item = (NavigationMenu)args.SelectedItem;
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
                case "Settings":
                    break;
            }
        }
    }
}

using GoodTimeStudio.MyPhone.Device.Services;
using GoodTimeStudio.MyPhone.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactsPage : Page
    {

        public ContactsPageViewModel ViewModel => (ContactsPageViewModel)DataContext;

        public ContactsPage()
        {
            InitializeComponent();
            Debug.Assert(App.Current.DeviceManager != null);
            DataContext = new ContactsPageViewModel(
                App.Current.DeviceManager.Services.GetRequiredService<IContactStore>(),
                App.Current.Services.GetRequiredService<ILogger<ContactViewModel>>());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Control event handler can not be static")]
        private void DetailsView_CommunicationsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                // Item is being recycled, make sure first item has no border
                if (args.ItemIndex == 0)
                {
                    var first = (ListViewItem)sender.ContainerFromIndex(0);
                    if (first != null)
                    {
                        first.BorderThickness = new Thickness(0);
                    }
                }
            }
            else if (args.ItemIndex == 0)
            {
                // A new first item
                ((ListViewItem)args.ItemContainer).BorderThickness = new Thickness(0);

                var second = (ListViewItem)sender.ContainerFromIndex(1);
                if (second != null)
                {
                    second.ClearValue(BorderThicknessProperty);
                }
            }
            else
            {
                // A new internal item
                ((ListViewItem)args.ItemContainer).ClearValue(BorderThicknessProperty);
            }
        }
    }
}

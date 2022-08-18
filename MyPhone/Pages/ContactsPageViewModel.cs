using CommunityToolkit.Mvvm.ComponentModel;
using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Device.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Pages
{
    public partial class ContactsPageViewModel : ObservableObject
    {
        private readonly IContactStore _contactStore;

        public ContactsPageViewModel(IContactStore contactStore)
        {
            _contactStore = contactStore;
            _contacts = new ObservableCollection<Contact>();
            _ = LoadContactsAsync();
        }

        [ObservableProperty]
        private ObservableCollection<Contact> _contacts;

        [ObservableProperty]
        private bool _loading;


        public async Task LoadContactsAsync()
        {
            try
            {
                Loading = true;
                Contacts = new ObservableCollection<Contact>(await _contactStore.GetAsync());
            }
            finally
            {
                Loading = false;
            }
        }
    }
}

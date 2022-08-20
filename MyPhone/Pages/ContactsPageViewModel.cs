using CommunityToolkit.Mvvm.ComponentModel;
using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Device.Services;
using GoodTimeStudio.MyPhone.Models;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ContactViewModel> _loggerContactViewModel;

        public ContactsPageViewModel(IContactStore contactStore,
            ILogger<ContactViewModel> loggerContactViewModel)
        {
            _contactStore = contactStore;
            _contacts = new ObservableCollection<ContactViewModel>();
            _loggerContactViewModel = loggerContactViewModel;
            _ = LoadContactsAsync();
        }

        [ObservableProperty]
        private ObservableCollection<ContactViewModel> _contacts;

        [ObservableProperty]
        private bool _loading;


        public async Task LoadContactsAsync()
        {
            try
            {
                Loading = true;
                var contacts = await _contactStore.GetAsync();
                Contacts = new ObservableCollection<ContactViewModel>(
                    contacts.Select(c => new ContactViewModel(c, _loggerContactViewModel)));
            }
            finally
            {
                Loading = false;
            }
        }
    }
}

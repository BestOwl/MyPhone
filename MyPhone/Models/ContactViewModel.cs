using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoodTimeStudio.MyPhone.Data;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Imaging;

namespace GoodTimeStudio.MyPhone.Models
{
    public partial class ContactViewModel : ObservableObject
    {
        private readonly ILogger<ContactViewModel> _logger;

        public ObservableCollection<ContactDetialInformation> Info { get; }

        public Contact Contact { get; }

        [ObservableProperty]
        private ImageSource? _photo;

        public ContactViewModel(Contact contact, ILogger<ContactViewModel> logger)
        {
            _logger = logger;
            Contact = contact;
            Info = new ObservableCollection<ContactDetialInformation>();
            ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

            if (contact.Detail.Telephones != null)
            {
                foreach (var phone in contact.Detail.Telephones)
                {
                    Info.Add(new ContactDetialInformation(
                        "\uE717", phone.Number,
                        resourceLoader.GetString("Contact_Phone"), 
                        primaryAction: Call,
                        primaryActionTooltip: resourceLoader.GetString("Contact_Call") + phone.Number,
                        secondaryAction: (i) => { /* TODO: Navigate to message page */ },
                        secondaryActionIcon: "\uE8BD",
                        secondaryActionTooltip: resourceLoader.GetString("Contact_SendMessage") + phone.Number));
                }
            }
            
            if (contact.Detail.Emails != null)
            {
                foreach (var email in contact.Detail.Emails)
                {
                    Info.Add(new ContactDetialInformation(
                        "\uE715", email.EmailAddress,
                        resourceLoader.GetString("Contact_Email")));
                }
            }

            if (!string.IsNullOrWhiteSpace(contact.Detail.Organization))
            {
                Info.Add(new ContactDetialInformation(
                    "\uE821", contact.Detail.Organization,
                    resourceLoader.GetString("Contact_Phone")));
            }

            if (!string.IsNullOrWhiteSpace(contact.Detail.OrganizationalUnit))
            {
                Info.Add(new ContactDetialInformation(
                    "\uE821", contact.Detail.OrganizationalUnit,
                    resourceLoader.GetString("Contact_Organizational_Unit")));
            }

            _ = LoadPhotoAsync();
        }

        public static async void Call(ContactDetialInformation info)
        {
            Debug.Assert(App.Current.DeviceManager != null);
            var callService = App.Current.DeviceManager.CallService;

            if (callService != null)
            {
                await callService.CallAsync(info.Value);
            }
            // else {  }
            // TODO: notify user when call service is unavailable.
        }

        public async Task LoadPhotoAsync()
        {
            var vCardPhoto = Contact.Detail.Photo;

            if (vCardPhoto != null)
            {
                if (vCardPhoto.IsEmbedded)
                {
                    try
                    {
                        BitmapImage image = new BitmapImage();
                        image.ImageFailed += Image_ImageFailed;

                        using (MemoryStream imageStream = new MemoryStream())
                        {
                            imageStream.Write(Convert.FromBase64String(vCardPhoto.Contents));
                            await imageStream.FlushAsync();
                            imageStream.Position = 0;
                            await image.SetSourceAsync(imageStream.AsRandomAccessStream());
                            Photo = image;
                        }
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogWarning(ex, "Malformed image content");
                    }
                }
                else
                {
                    Photo = new BitmapImage(new Uri(vCardPhoto.Contents));
                }
            }
        }

        private void Image_ImageFailed(object sender, Microsoft.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            _logger.LogWarning("Failed to load contact photo for {ContactFormattedName}: {ErrorMessage}", 
                Contact.FormattedName, e.ErrorMessage);
        }
    }

    public partial class ContactDetialInformation
    {
        public string Icon { get; }

        public string Value { get; }

        public string Type { get; }

        public Action<ContactDetialInformation>? PrimaryAction { get; }

        public string? PrimaryActionTooltip { get; }

        public bool HasSecondaryAction { get => SecondaryAction != null; }

        public Action<ContactDetialInformation>? SecondaryAction { get; }

        public string SecondaryActionIcon { get; }

        public string? SecondaryActionTooltip { get; }

        public ContactDetialInformation(string icon, string value, string type, 
            Action<ContactDetialInformation>? primaryAction = null,
            string? primaryActionTooltip = null, 
            Action<ContactDetialInformation>? secondaryAction = null,
            string secondaryActionIcon = "",
            string? secondaryActionTooltip = null)
        {
            Icon = icon ?? throw new ArgumentNullException(nameof(icon));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            PrimaryAction = primaryAction;
            PrimaryActionTooltip = primaryActionTooltip;
            SecondaryAction = secondaryAction;
            SecondaryActionIcon = secondaryActionIcon;
            SecondaryActionTooltip = secondaryActionTooltip;
        }

        [RelayCommand]
        private void CopyValue()
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Value);
            Clipboard.SetContent(dataPackage);
        }

        [RelayCommand]
        private void DoPrimaryAction()
        {
            PrimaryAction?.Invoke(this);
        }

        [RelayCommand]
        private void DoSecondaryAction()
        {
            SecondaryAction?.Invoke(this);
        }
    }
}

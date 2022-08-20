using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Pages
{
    public partial class CallPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? phoneNumber;

        [ObservableProperty]
        private int selectionLength;

        [ObservableProperty]
        private int selectionStart;

        /// <summary>
        /// Raise when user input phone number with button (no including keyboard)
        /// </summary>
        public event EventHandler<EventArgs>? PhoneNumberInputFocus;

        [RelayCommand]
        public void PressDigit(string digit)
        {
            PhoneNumberInputFocus?.Invoke(this, new EventArgs());
            if (SelectionLength != 0)
            {
                // We need to store it because when assigning a new value to PhoneNumber, the UI control will reset this 
                // Note: it will not be reset on unit test environment
                int start = SelectionStart;
                PhoneNumber = PhoneNumber!.Remove(SelectionStart, SelectionLength); // SelectionLength != 0 will gurantee PhoneNumber not null
                SelectionLength = 0;
                SelectionStart = start;
            }

            if (PhoneNumber != null && SelectionStart != PhoneNumber.Length)
            {
                PhoneNumber = PhoneNumber.Insert(SelectionStart, digit);
            }
            else
            {
                PhoneNumber += digit;
            }

            SelectionStart += 1;
        }

        [RelayCommand]
        public void PressBackSpace()
        {
            PhoneNumberInputFocus?.Invoke(this, new EventArgs());
            if (PhoneNumber != null)
            {
                int pos = SelectionStart;
                if (SelectionLength != 0)
                {
                    // There is a bug in SelectionBindingTextBox. 
                    // This dummy operation keep the SelectionStart in the right position after PhoneNumber.Remove()
                    SelectionStart = SelectionStart + SelectionLength; // DO NOT REMOVE THIS LINE

                    PhoneNumber = PhoneNumber!.Remove(pos, SelectionLength); // SelectionLength != 0 will gurantee PhoneNumber not null
                    SelectionLength = 0;
                    SelectionStart = pos;
                }
                else
                {
                    if (pos > 0)
                    {
                        PhoneNumber = PhoneNumber.Remove(pos - 1, 1);
                        SelectionStart = pos - 1;
                    }
                }
            }
        }

        [RelayCommand]
        public async Task Call()
        {
            Debug.Assert(App.Current.DeviceManager != null);
            var deviceManager = App.Current.DeviceManager;

            if (PhoneNumber != null)
            {
                if (deviceManager.CallService != null)
                {
                    await deviceManager.CallService.CallAsync(PhoneNumber);
                }
                else
                {
                    // TODO: what if CallService is not available 
                }
            }
        }

    }
}

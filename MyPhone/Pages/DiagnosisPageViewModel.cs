using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoodTimeStudio.MyPhone.Device;
using GoodTimeStudio.MyPhone.Models;
using GoodTimeStudio.MyPhone.Utilities;
using Microsoft.UI.Dispatching;
using MyPhone.OBEX;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Pages
{
    public partial class DiagnosisPageViewModel : ObservableObject
    {
        public ObservableBluetoothDevice BluetoothDevice { get; }
        public ObservableCollection<ObservablePhoneLine> PhoneLines { get; }

        public bool CallServiceSupported => _deviceManager.CallService != null;
        public bool SmsServiceSupported => _deviceManager.SmsService != null;
#pragma warning disable CA1822 // Mark members as static
        public bool PhonebookServiceSupported => false;
#pragma warning restore CA1822 // Mark members as static

        #region Services State Overview
        [ObservableProperty]
        private DeviceServiceProviderInformation _callServiceInfo;

        [ObservableProperty]
        private DeviceServiceProviderInformation _smsServiceInfo;

        [ObservableProperty]
        private DeviceServiceProviderInformation _phonebookServiceInfo;
        #endregion

        #region Call Service Additional Properties
        [ObservableProperty]
        private string? _phoneLineTransportDeviceId;

        [ObservableProperty]
        private bool _isPhoneLineRegistered;

        [ObservableProperty]
        private string _autoSelectedPhoneLineId;

        [AlsoNotifyChangeFor(nameof(SelectedPhoneLineId))]
        [ObservableProperty]
        private ObservablePhoneLine? _selectedPhoneLine;

        public string SelectedPhoneLineId => SelectedPhoneLine != null ? SelectedPhoneLine.Id.ToString() : "(none)";

        [ObservableProperty]
        private string? _phoneNumberBoxInputString;
        #endregion

        #region SMS Service Additional Properties
        [ObservableProperty]
        private string? _messageHandleInputString;
        #endregion

        private readonly DeviceManager _deviceManager;
        private Task<PhoneLineWatcher>? _createPhoneLineWatcherTask;
        [DisallowNull]
        private PhoneLineWatcher? _phoneLineWatcher;
        private DispatcherQueue _dispatcherQueue;

        public DiagnosisPageViewModel()
        {
            Debug.Assert(App.Current.DeviceManager != null);
            _deviceManager = App.Current.DeviceManager;

            Debug.Assert(_deviceManager.CurrentDevice != null);

            BluetoothDevice = new ObservableBluetoothDevice(_deviceManager.CurrentDevice);
            PhoneLines = new ObservableCollection<ObservablePhoneLine>();
            _autoSelectedPhoneLineId = "(none)";
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            _callServiceInfo = new DeviceServiceProviderInformation();
            _smsServiceInfo = new DeviceServiceProviderInformation();
            _phonebookServiceInfo = new DeviceServiceProviderInformation();

            if (_deviceManager.CallService != null)
            {
                CallServiceInfo.State = _deviceManager.CallService.State;
                _createPhoneLineWatcherTask = DeviceCallServiceProvider.CreatePhoneLineWatcherAsync();
                _deviceManager.CallService.ServiceProdiverStateChanged += CallService_ServiceProdiverStateChanged;
                InitCallServiceInfo();
                UpdateServiceInfo(_deviceManager.CallService, CallServiceInfo);
            }
            if (_deviceManager.SmsService != null)
            {
                SmsServiceInfo.State = _deviceManager.SmsService.State;
                _deviceManager.SmsService.ServiceProdiverStateChanged += SmsService_ServiceProdiverStateChanged;
            }
        }

        public async void OnNavigatedTo()
        {
            if (_createPhoneLineWatcherTask != null)
            {
                _phoneLineWatcher = await _createPhoneLineWatcherTask;
                _phoneLineWatcher.LineAdded += _phoneLineWatcher_LineAdded;
                _phoneLineWatcher.LineRemoved += _phoneLineWatcher_LineRemoved;
                _createPhoneLineWatcherTask = null;
            }

            if (_phoneLineWatcher != null)
            {
                _phoneLineWatcher.Start();
            }
        }

        public void OnNavigatedFrom()
        {
            if (_phoneLineWatcher != null)
            {
                _phoneLineWatcher.Stop();
            }
        }

        private async void InitCallServiceInfo()
        {
            Debug.Assert(_deviceManager.CallService != null);

            PhoneLineTransportDeviceId = _deviceManager.CallService.TransportDevice.DeviceId;
            IsPhoneLineRegistered = _deviceManager.CallService.TransportDevice.IsRegistered();
            PhoneLine? pl = await _deviceManager.CallService.GetSelectedPhoneLineAsync();
            if (pl != null)
            {
                _autoSelectedPhoneLineId = pl.Id.ToString();
            }
        }

        #region PhoneLineWatcher events
        private void _phoneLineWatcher_LineRemoved(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PhoneLines.Remove(pl => pl.Id == args.LineId);
            });
        }

        private void _phoneLineWatcher_LineAdded(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            _dispatcherQueue.TryEnqueue(async () => 
            {
                PhoneLine phoneLine = await PhoneLine.FromIdAsync(args.LineId);
                PhoneLines.Add(new ObservablePhoneLine(phoneLine));
            });
        }
        #endregion

        private static string GetNextRetryTimeDescriptionText(DateTime? nextRetryTime)
        {
            return $"Retry scheduled: {nextRetryTime}";
        }

        private static void UpdateServiceInfo(BaseDeviceServiceProvider serviceProvider, DeviceServiceProviderInformation info)
        {
            info.State = serviceProvider.State;

            switch (info.State)
            {
                case DeviceServiceProviderState.RetryScheduled:
                    info.StatusMessage = GetNextRetryTimeDescriptionText(serviceProvider.NextRetryTime);
                    break;
                case DeviceServiceProviderState.Idle:
                case DeviceServiceProviderState.Connecting:
                case DeviceServiceProviderState.Connected:
                    info.StatusMessage = string.Empty;
                    break;
                case DeviceServiceProviderState.Stopped:
                    if (serviceProvider.StopReason != null)
                    {
                        info.StatusMessage = serviceProvider.StopReason.Message;
                    }
                    else
                    {
                        info.StatusMessage = string.Empty;
                    }
                    break;
            }
        }

        private void CallService_ServiceProdiverStateChanged(object? sender, DeviceServiceProviderState e)
        {
            var callService = _deviceManager.CallService;
            Debug.Assert(callService != null);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateServiceInfo(callService, CallServiceInfo);
            });
        }

        private void SmsService_ServiceProdiverStateChanged(object? sender, DeviceServiceProviderState e)
        {
            var smsService = _deviceManager.SmsService;
            Debug.Assert(smsService != null);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateServiceInfo(smsService, SmsServiceInfo);
            });
        }

        [ICommand]
        private void Call()
        {
            if (SelectedPhoneLine != null && !string.IsNullOrEmpty(PhoneNumberBoxInputString))
            {
                SelectedPhoneLine.PhoneLine.Dial(PhoneNumberBoxInputString, PhoneNumberBoxInputString);
            }
        }

        [ICommand]
        private async Task GetMessagesListing()
        {
            var handles = await _deviceManager.SmsService!.MasClient!.GetMessageListing(1024, "telecom");
            Debug.WriteLine($"Handle count: {handles.Count}");
        }

        [ICommand]
        private async Task GetMessageByHandle()
        {
            if (!string.IsNullOrEmpty(MessageHandleInputString))
            {
                BMessage message = await _deviceManager.SmsService!.MasClient!.GetMessageAsync(MessageHandleInputString);
                Debug.WriteLine($"Sender: {message.Sender.FormattedName}");
                Debug.WriteLine("Body: ");
                Debug.WriteLine(message.Body);
            }
        }
    }
}

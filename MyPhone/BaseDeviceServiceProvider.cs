using GoodTimeStudio.MyPhone.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone
{
    public abstract class BaseDeviceServiceProvider : IDisposable
    {
        public event EventHandler<DeviceServiceProviderState>? ServiceProdiverStateChanged;

        private DeviceServiceProviderState _state = DeviceServiceProviderState.Stopped;
        public DeviceServiceProviderState State
        {
            get => _state;
            set
            {
                _state = value;
                ServiceProdiverStateChanged?.Invoke(this, _state);
            }
        }
        public DeviceServiceException? StopReason { get; private set; }

        private DynamicTimer? _retryTimer;
        private static DynamicTimerSchedule[] s_schedules = new DynamicTimerSchedule[]
        {
            new DynamicTimerSchedule(TimeSpan.FromSeconds(15), 4), // T+1min
            new DynamicTimerSchedule(TimeSpan.FromSeconds(30), 10), // T+1min -> T+6min (5min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(1), 9), // T+6min -> T+15min (9min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(3), 5), // T+15min -> T+30min (15min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(5), 6), // T+30min -> T+60min (30min in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(10), 6), // T+1h -> T+2h (1h in total)
            new DynamicTimerSchedule(TimeSpan.FromMinutes(30), 2), // T+2h -> T+3h (1min in total)
            new DynamicTimerSchedule(TimeSpan.FromHours(1), 0), // T+3h -> 
        };

        protected BluetoothDevice BluetoothDevice { get; private set; }

        public BaseDeviceServiceProvider(BluetoothDevice bluetoothDevice)
        {
            BluetoothDevice = bluetoothDevice;
        }

        /// <summary>
        /// Connect to the device service.
        /// </summary>
        /// <returns>Whether the device service is successfully connected</returns>
        /// <remarks>This is a public warpper method for <see cref="ConnectToServiceAsync"/></remarks>
        public async Task<bool> ConnectAsync()
        {
            bool success = await ConnectToServiceAsync();
            if (success)
            {
                State = DeviceServiceProviderState.Connected;
                RegisterServiceConnectionLostEventHandler();
            }
            else
            {
                ScheduleReconnect();
            }
            return success;
        }

        /// <summary>
        /// Connect to the device service. The core implementation of <see cref="ConnectAsync"/>. 
        /// <see cref="ConnectAsync"/> is a public wrapper method, it will call this method to actually connect to 
        /// the device service.
        /// </summary>
        /// <exception cref="DeviceServiceException">
        /// Throws when the device not longer supports this service (e.g. when the user turns off the service on their device). 
        /// If this exception is thrown, the service provider will turn into stopped state and will not try to auto-reconnect.
        /// </exception>
        /// <returns>Whether the device service is successfully connected</returns>
        protected abstract Task<bool> ConnectToServiceAsync();

        /// <summary>
        /// Register event handler to listen connection lost event. You can override to implement your own detection method
        /// </summary>
        protected virtual void RegisterServiceConnectionLostEventHandler()
        {
            BluetoothDevice.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
        }

        private void BluetoothDevice_ConnectionStatusChanged(BluetoothDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                OnDisconnected();
            }
        }

        /// <summary>
        /// Call this method when the service connection is lost
        /// </summary>
        protected virtual void OnDisconnected()
        {
            State = DeviceServiceProviderState.Retrying;
            ScheduleReconnect();
        }

        private void ScheduleReconnect()
        {
            _retryTimer = new DynamicTimer(s_schedules);
            _retryTimer.Elapsed += _reconnectTimer_Elapsed;
            _retryTimer.Start();
        }

        private async void _reconnectTimer_Elapsed(object? sender, DynamicTimerElapsedEventArgs e)
        {
            Debug.Assert(_retryTimer != null);
            try
            {
                if (await ConnectAsync())
                {
                    _retryTimer.Stop();
                }
            }
            catch (DeviceServiceException ex)
            {
                _retryTimer.Stop();
                StopReason = ex;
                State = DeviceServiceProviderState.Stopped;
            }
        }

        public virtual void Dispose()
        {
            _retryTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class DeviceServiceException : Exception
    {
        public DeviceServiceException()
        {
        }

        public DeviceServiceException(string? message) : base(message)
        {
        }

        public DeviceServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }

    public enum DeviceServiceProviderState
    {
        Stopped,
        Connected,
        Retrying
    }
}

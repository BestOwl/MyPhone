using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Device.Services
{
    /// <summary>
    /// The <see langword="interface"/> for getting and setting device configuration.
    /// </summary>
    public interface IDeviceConfiguration
    {
        string? DeviceId { get; set; }

        DateTime? SmsServiceLastSyncedTime { get; set; }

        DateTime? PhonebookServiceSyncedTime { get; set; }

        TimeSpan? SyncTimeSpan { get; set; }
    }
}

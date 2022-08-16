using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Data
{
    public class DeviceConfiguration
    {
        public int Id { get; set; }

        public string? DeviceId { get; set; } = null!;

        public DateTime? SmsServiceLastSyncedTime { get; set; }

        public DateTime? PhonebookServiceLastSyncedTime { get; set; }

        public TimeSpan? SyncTimeSpan { get; set; }
    }
}

using GoodTimeStudio.MyPhone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Device.Services
{
    public class EntityFrameworkDeviceConfiguration : IDeviceConfiguration
    {
        private readonly DeviceDbContext _context;

        public EntityFrameworkDeviceConfiguration(DeviceDbContext context)
        {
            _context = context;
        }

        public string DeviceId 
        { 
            get => _context.Configurations.First().DeviceId;
            set
            {
                _context.Configurations.First().DeviceId = value;
                _context.SaveChanges();
            }
        }

        public DateTime SmsServiceLastSyncedTime 
        {
            get => _context.Configurations.First().SmsServiceLastSyncedTime;
            set
            {
                _context.Configurations.First().SmsServiceLastSyncedTime = value;
                _context.SaveChanges();
            } 
        }

        public DateTime PhonebookServiceSyncedTime 
        {
            get => _context.Configurations.First().PhonebookServiceLastSyncedTime;
            set
            {
                _context.Configurations.First().PhonebookServiceLastSyncedTime = value;
                _context.SaveChanges();
            } 
        }

        public TimeSpan SyncTimeSpan 
        {
            get => _context.Configurations.First().SyncTimeSpan;
            set
            {
                _context.Configurations.First().SyncTimeSpan = value;
                _context.SaveChanges();
            } 
        }
    }
}

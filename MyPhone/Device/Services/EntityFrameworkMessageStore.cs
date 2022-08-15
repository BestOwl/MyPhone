using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.OBEX;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Device.Services
{
    public class EntityFrameworkMessageStore : IMessageStore
    {
        private readonly DeviceDbContext _context;

        public EntityFrameworkMessageStore(DeviceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Contains(string handle)
        {
            return await _context.Messages.FindAsync(handle) != null;
        }

        public async Task<Message?> GetAsync(string handle)
        {
            return await _context.Messages.FindAsync(handle);
        }

        public async Task<IEnumerable<Message>> GetAsync(int pageIndex, int pageSize)
        {
            return await _context.Messages
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task SaveAsync(Message data)
        {
            await _context.Messages.AddAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync(IEnumerable<Message> data)
        {
            await _context.Messages.AddRangeAsync(data);
            await _context.SaveChangesAsync();
        }
    }

    public static class EntityFrameworkMessageStoreExtensions
    {
        public static IServiceCollection AddEntityFrameworkMessageStore(this IServiceCollection services)
        {
            services.AddScoped<IMessageStore, EntityFrameworkMessageStore>();
            return services;
        }
    }
}

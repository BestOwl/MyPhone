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

namespace GoodTimeStudio.MyPhone.Services
{
    public class EntityFrameworkMessageStore : IMessageStore
    {
        private readonly DeviceDbContext _context;

        public EntityFrameworkMessageStore(DeviceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Contains(string messageHandle)
        {
            return await _context.Messages.FindAsync(messageHandle) != null;
        }

        public async Task<Message?> GetMessageAsync(string messageHandle)
        {
            return await _context.Messages.FindAsync(messageHandle);
        }

        public async Task<IEnumerable<Message>> GetMessageAsync(int pageIndex, int pageSize)
        {
            return await _context.Messages
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task SaveMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync(); 
        }

        public async Task SaveMessageAsync(IEnumerable<Message> messages)
        {
            await _context.Messages.AddRangeAsync(messages);
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

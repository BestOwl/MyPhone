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

        public Task<BMessage> GetMessageAsync(string messageHandle)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BMessage>> GetMessageAsync(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task SaveMessageAsync(BMessage message)
        {
            throw new NotImplementedException();
        }

        public async Task SaveMessageAsync(IEnumerable<BMessage> messages)
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

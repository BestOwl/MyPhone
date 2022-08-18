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
    public abstract class EntityFrameworkDataStore<TData, TKey> : IDataStore<TData, TKey> where TData : class, IIdentifiable<TKey>
    {
        protected DeviceDbContext Context { get; }

        public EntityFrameworkDataStore(DeviceDbContext context)
        {
            Context = context;
        }

        public abstract DbSet<TData> Datas { get; }

        public async Task ClearStoreAsync()
        {
            Datas.RemoveRange(Datas);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> ContainsAsync(TKey dataId)
        {
            return await Datas.FindAsync(dataId) != null;
        }

        public async Task<TData?> GetByIdAsync(TKey dataId)
        {
            return await Datas.FindAsync(dataId);
        }

        public async Task<IEnumerable<TData>> GetAsync(int pageIndex, int pageSize)
        {
            return await Datas
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<TData>> GetAsync()
        {
            return await Datas.ToListAsync();
        }

        public async Task AddAsync(TData data)
        {
            await Datas.AddAsync(data);
            await Context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TData> datas)
        {
            await Datas.AddRangeAsync(datas);
            await Context.SaveChangesAsync();
        }
    }

    public class EntityFrameworkMessageStore : EntityFrameworkDataStore<Message, string>, IMessageStore
    {
        public EntityFrameworkMessageStore(DeviceDbContext context) : base(context)
        {
        }

        public override DbSet<Message> Datas => Context.Messages;
    }

    public class EntityFrameworkContactStore : EntityFrameworkDataStore<Contact, string>, IContactStore
    {
        public EntityFrameworkContactStore(DeviceDbContext context) : base(context)
        {
        }

        public override DbSet<Contact> Datas => Context.Contacts;
    }
}

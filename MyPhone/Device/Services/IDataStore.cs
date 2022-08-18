using GoodTimeStudio.MyPhone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Device.Services
{
    public interface IDataStore<TData, TKey> where TData : IIdentifiable<TKey>
    {
        Task AddAsync(TData data);

        Task AddRangeAsync(IEnumerable<TData> datas);

        Task<TData?> GetByIdAsync(TKey dataId);

        Task<IEnumerable<TData>> GetAsync(int pageIndex, int pageSize);

        Task<IEnumerable<TData>> GetAsync();

        Task<bool> ContainsAsync(TKey dataId);

        Task ClearStoreAsync();
    }

    public interface IMessageStore : IDataStore<Message, string>
    {
    }

    public interface IContactStore : IDataStore<Contact, string>
    {
    }
}

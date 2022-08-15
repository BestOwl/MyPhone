using GoodTimeStudio.MyPhone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Device.Services
{
    public interface IDataStore<TData, TDataHandle>
    {
        Task SaveAsync(TData data);

        Task SaveAsync(IEnumerable<TData> datas);

        Task<TData?> GetAsync(TDataHandle handle);

        Task<IEnumerable<TData>> GetAsync(int pageIndex, int pageSize);

        Task<bool> Contains(TDataHandle handle);
    }

    public interface IMessageStore : IDataStore<Message, string>
    {
    }

    public interface IContactStore : IDataStore<Contact, string>
    {
    }
}

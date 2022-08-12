using GoodTimeStudio.MyPhone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Services
{
    public interface IMessageStore
    {
        Task SaveMessageAsync(Message message);

        Task SaveMessageAsync(IEnumerable<Message> messages);

        Task<Message?> GetMessageAsync(string messageHandle);

        Task<IEnumerable<Message>> GetMessageAsync(int pageIndex, int pageSize);

        Task<bool> Contains(string messageHandle);
    }
}

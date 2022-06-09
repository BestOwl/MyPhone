using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Services
{
    public interface IMessageStore
    {
        Task SaveMessageAsync(BMessage message);

        Task SaveMessageAsync(IEnumerable<BMessage> messages);

        Task<BMessage> GetMessageAsync(string messageHandle);

        Task<IEnumerable<BMessage>> GetMessageAsync(int pageIndex, int pageSize);
    }
}

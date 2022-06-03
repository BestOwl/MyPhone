using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Services
{
    public interface IMessageNotificationService
    {
        void ShowMessageNotification(BMessage message);
    }
}

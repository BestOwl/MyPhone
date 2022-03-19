using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Services
{
    public class AppDispatcherService : IAppDispatcherService
    {
        public DispatcherQueue GetDispatcherQueue()
        {
            return MainWindow.Instance.DispatcherQueue;
        }
    }
}

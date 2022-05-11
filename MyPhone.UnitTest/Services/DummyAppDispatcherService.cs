using GoodTimeStudio.MyPhone.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPhone.UnitTest.Services
{
    internal class DummyAppDispatcherService : IAppDispatcherService
    {
        public DispatcherQueue GetDispatcherQueue()
        {
            throw new NotImplementedException();
        }
    }
}

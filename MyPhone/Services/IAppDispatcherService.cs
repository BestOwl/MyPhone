using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace GoodTimeStudio.MyPhone.Services
{
    /// <summary>
    /// The service <see langword="interface"/> of providing access to the application UI thread's <see cref="DispatcherQueue"/>
    /// </summary>
    public interface IAppDispatcherService
    {
        /// <summary>
        /// Get <see cref="DispatcherQueue"/> from UI thread
        /// </summary>
        /// <returns></returns>
        DispatcherQueue GetDispatcherQueue();
    }
}

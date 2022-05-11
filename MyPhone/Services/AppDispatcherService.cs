using Microsoft.UI.Dispatching;

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

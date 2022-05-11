using GoodTimeStudio.MyPhone.Pages.Call;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.DependencyInjection;
using MyPhone.UnitTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPhone.UnitTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAppDispatcherService, DummyAppDispatcherService>();
            services.AddSingleton<IDeviceService, DummyDeviceService>();

            services.AddTransient<CallPageViewModel>();
        }
    }
}

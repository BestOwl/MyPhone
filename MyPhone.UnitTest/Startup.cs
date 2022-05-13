using GoodTimeStudio.MyPhone;
using GoodTimeStudio.MyPhone.Pages.Call;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.DependencyInjection;
using MyPhone.UnitTest.Services;

namespace MyPhone.UnitTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDeviceService, DummyDeviceService>();
            services.AddSingleton<DeviceManager>();
            services.AddTransient<CallPageViewModel>();
        }
    }
}

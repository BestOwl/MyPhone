using GoodTimeStudio.MyPhone;
using GoodTimeStudio.MyPhone.Device;
using GoodTimeStudio.MyPhone.Pages;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.DependencyInjection;
using MyPhone.UnitTest.Services;

namespace MyPhone.UnitTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDevicePairingService, DummyDeviceService>();
            services.AddTransient<CallPageViewModel>();
        }
    }
}

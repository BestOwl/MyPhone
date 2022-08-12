using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Pages;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPhone.UnitTest.Services;

namespace MyPhone.UnitTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DeviceDbContext>(options => options.UseSqlite($"Data Source=Test.db"));
            services.AddSingleton<IDevicePairingService, DummyDeviceService>();
            services.AddTransient<CallPageViewModel>();
        }
    }
}

using CommunityToolkit.WinUI.Notifications;
using Microsoft.Extensions.DependencyInjection;
using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Services
{
    public class MessageToastNotificationService : IMessageNotificationService
    {
        public void ShowMessageNotification(BMessage message)
        {
            new ToastContentBuilder()
                .AddText(message.Sender.FormattedName)
                .AddText(message.Body)
                .Show();
        }
    }

    public static class MessageToastNotificationServiceExtensions
    {
        public static IServiceCollection AddMessageToastNotification(this IServiceCollection services)
        {
            services.AddTransient<IMessageNotificationService, MessageToastNotificationService>();
            return services;
        }
    }
}

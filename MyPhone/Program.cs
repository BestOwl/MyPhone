using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

            // As of Windows App SDK 1.1.4, async main will causes Clipboard API not working
            // https://docs.microsoft.com/en-us/answers/questions/780749/winui-3-clipboard-api-not-working.html
            bool isRedirect = DecideRedirect().Result;

            if (!isRedirect)
            {
                Launch();
            }
        }

        private static void Launch()
        {
            Microsoft.UI.Xaml.Application.Start(p =>
            {
                DispatcherQueueSynchronizationContext context = new(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }

        // Single-instance redirect, redirect Activated to the main instance 
        private static async Task<bool> DecideRedirect()
        {
            bool isRedirect = false;
            AppInstance mainInstance = AppInstance.FindOrRegisterForKey("main");
            AppInstance currentInstance = AppInstance.GetCurrent();
            AppActivationArguments activationArgs = currentInstance.GetActivatedEventArgs();
            if (mainInstance.IsCurrent)
            {

            }
            else
            {
                isRedirect = true;
                await mainInstance.RedirectActivationToAsync(activationArgs);
            }

            return isRedirect;
        }
    }
}

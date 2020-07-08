using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace GoodTimeStudio.MyPhone
{
    public class AppServiceManager
    {
        public static AppServiceConnection appServiceConnection;
        private static BackgroundTaskDeferral appServiceDeferral;
        public static event EventHandler AppServiceConnected;

        //App Service
        // see https://blogs.msdn.microsoft.com/appconsult/2016/12/19/desktop-bridge-the-migrate-phase-invoking-a-win32-process-from-a-uwp-app/
        public static void OnAppServiceActivated(BackgroundActivatedEventArgs args)
        {
            IBackgroundTaskInstance taskInstance = args.TaskInstance;
            AppServiceTriggerDetails details = (AppServiceTriggerDetails)args.TaskInstance.TriggerDetails;
            appServiceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnAppServiceCanceled;
            appServiceConnection = details.AppServiceConnection;
            appServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
            AppServiceConnected?.Invoke(null, null);
        }

        private static void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (args.Request.Message.TryGetValue("request", out object obj))
            {
                string message = null;
                if (obj  is string)
                {
                    message = (string)obj;
                }
                else
                {
                    return;
                }

                switch (message)
                {
                    case "exit":
                        //Try close the App
                        //await ApplicationView.GetForCurrentView().TryConsolidateAsync(); // Try to suspend the app
                        App.Current.Exit();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnAppServiceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            appServiceDeferral?.Complete();
        }

    }
}

using GoodTimeStudio.MyPhone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GoodTimeStudio.MyPhone.Services
{
    public class DevicePairDialogService : IDevicePairDialogService
    {
        private DevicePairDialog? dialog;
        private Timer? timer;

        private readonly IAppDispatcherService appDispatcherService;

        public DevicePairDialogService(IAppDispatcherService appDispatcherService)
        {
            this.appDispatcherService = appDispatcherService;
        }

        public async Task<bool> ShowPairDialogAsync(string deviceName, string pairPIN, TimeSpan? timeout = null)
        {
            dialog = new DevicePairDialog(deviceName, pairPIN);

            // Currently, to show a content dialog in WinUI3, you must manually set the XamlRoot on the dialog to the root of the XAML host
            // https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.contentdialog?view=winui-3.0#contentdialog-in-appwindow-or-xaml-islands
            dialog.XamlRoot = MainWindow.Instance.XamlRoot;

            if (timeout != null)
            {
                timer = new Timer(timeout.Value.TotalMilliseconds);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }

            var result = await dialog.ShowAsync();
            return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            timer!.Stop();
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
            HideDialog();
        }

        public void HideDialog()
        {
            appDispatcherService.GetDispatcherQueue().TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                dialog?.Hide();
            });
        }

    }
}

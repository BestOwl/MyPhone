using GoodTimeStudio.MyPhone.Controls;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace GoodTimeStudio.MyPhone.Services
{
    public sealed class DevicePairDialogService : IDevicePairDialogService, IDisposable
    {
        private DevicePairDialog? dialog;
        private Timer? _timer;

        public async Task<bool> ShowPairDialogAsync(string deviceName, string pairPIN, TimeSpan? timeout = null)
        {
            TaskCompletionSource<ContentDialogResult> tcs = new TaskCompletionSource<ContentDialogResult>();
            MainWindow.WindowDispatcher.TryEnqueue(async () =>
            {
                dialog = new DevicePairDialog(deviceName, pairPIN);

                // Currently, to show a content dialog in WinUI3, you must manually set the XamlRoot on the dialog to the root of the XAML host
                // https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.contentdialog?view=winui-3.0#contentdialog-in-appwindow-or-xaml-islands
                dialog.XamlRoot = MainWindow.XamlRoot;

                if (timeout != null)
                {
                    _timer = new Timer(timeout.Value.TotalMilliseconds);
                    _timer.Elapsed += Timer_Elapsed;
                    _timer.Start();
                }

                var result = await dialog.ShowAsync();
                tcs.SetResult(result);
            });

            return await tcs.Task == ContentDialogResult.Primary;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _timer!.Stop();
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Dispose();
            HideDialog();
        }

        public void HideDialog()
        {
            MainWindow.WindowDispatcher.TryEnqueue(() =>
            {
                dialog?.Hide();
            });
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}

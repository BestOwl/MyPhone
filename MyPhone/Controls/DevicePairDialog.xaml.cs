using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone.Controls
{
    public sealed partial class DevicePairDialog : ContentDialog
    {
        //private bool closed;
        //private Timer? timer;

        public DevicePairDialog(string deviceName, string pin)
        {
            this.InitializeComponent();
            Title = "Pair " + deviceName;
            PairDialogPin.Text = pin;
            PairDialogTip.Text = "Press Connect if the PIN on \"" + deviceName + "\" matchs this one.";
            //closed = false;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            //timer = new Timer(10000); // 10 seconds
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();
        }

        //private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        //{
        //    timer!.Stop();
        //    timer.Elapsed -= Timer_Elapsed;

        //    if (!closed)
        //    {
        //        MainWindow.Instance.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
        //            Hide();
        //        });
        //    }
        //}

        private void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //closed = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPhone.TrayApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
             * Using XamlIsland to host WinUI controls in NotifyIcon menu
             * 
             * See also:
             * https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/host-standard-control-with-xaml-islands
             */
            //using (new XamlHostApp()) // don't understand why "using", just follow the docs the link above
            //{
                
            //}

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyPhoneTray());
        }
    }
}

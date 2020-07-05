using Microsoft.Toolkit.Forms.UI.XamlHost;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyPhone.TrayApp
{
    public partial class MyPhoneTray : Form
    {
        MenuFlyout _ContextMenu;
        NotifyIcon _NotifyIcon;

        public MyPhoneTray()
        {
            InitializeComponent();

            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            AllowTransparency = true;
            this.WindowState = FormWindowState.Minimized;
            //Opacity = 0; 

            this.components = new Container();
            _NotifyIcon = new NotifyIcon(this.components);
            _NotifyIcon.Icon = this.Icon;
            _NotifyIcon.Text = "My Phone Assistant";
            _NotifyIcon.Visible = true;
            _NotifyIcon.MouseClick += _NotifyIcon_MouseClick;

            WindowsXamlHost host = new WindowsXamlHost();
            host.InitialTypeName = typeof(Grid).FullName;
            host.ChildChanged += Host_ChildChanged;
            host.AutoSize = true;
            host.Location = new Point(0, 0);
            Controls.Add(host);
        }

        private void _NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point mousePoint = System.Windows.Forms.Control.MousePosition;
                mousePoint.Offset(0, -100);
                Location = mousePoint;
                this.WindowState = FormWindowState.Normal;
            }
            else if (e.Button == MouseButtons.Left)
            {

            }
        }

        private void Host_ChildChanged(object sender, EventArgs e)
        {
            WindowsXamlHost windowsXamlHost = (WindowsXamlHost) sender;
            Grid grid = (Grid) windowsXamlHost.Child;

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Hello world! Xaml Island!";
            grid.Children.Add(textBlock);

            _ContextMenu = new MenuFlyout();
            _ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Exit" });
        }

    }
}

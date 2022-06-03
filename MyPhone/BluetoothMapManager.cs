using CommunityToolkit.Mvvm.DependencyInjection;
using GoodTimeStudio.MyPhone.Services;
using MyPhone.OBEX;
using MyPhone.OBEX.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// Bluetooth Message Access Profile Manager
    /// </summary>
    public class BluetoothMapManager
    {
        private readonly BluetoothDevice _device;
        private readonly IMessageNotificationService _notificationService;

        private BluetoothMasClientSession _masClientSession;
        private BluetoothMnsServerSession _mnsServerSession;

        public BluetoothMapManager(BluetoothDevice bluetoothDevice)
        {
            _device = bluetoothDevice;
            _notificationService = Ioc.Default.GetRequiredService<IMessageNotificationService>();
            _masClientSession = new BluetoothMasClientSession(bluetoothDevice);
            _mnsServerSession = new BluetoothMnsServerSession();
        }

        /// <summary>
        /// Connect to the MAS service and start the MNS server
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            await _masClientSession.ConnectAsync();
            await _mnsServerSession.StartServerAsync();

            Debug.Assert(_masClientSession.ObexClient != null);
            await _masClientSession.ObexClient.SetNotificationRegistration(true);
            _mnsServerSession.ClientAccepted += _mnsServerSession_ClientAccepted;
        }

        private void _mnsServerSession_ClientAccepted(BluetoothObexServerSession<MnsServer> sender, BluetoothObexServerSessionClientAcceptedEventArgs<MnsServer> e)
        {
            e.ObexServer.MessageReceived += ObexServer_MessageReceived;
        }

        private async void ObexServer_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Assert(_masClientSession.ObexClient != null);
            BMessage message = await _masClientSession.ObexClient.GetMessageAsync(e.MessageHandle);
            _notificationService.ShowMessageNotification(message);
            Debug.WriteLine(message.Body);
        }
    }
}

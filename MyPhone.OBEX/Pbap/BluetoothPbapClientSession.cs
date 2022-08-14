using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using System;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.OBEX.Pbap
{
    public class BluetoothPbapClientSession : BluetoothObexClientSession<PbapClient>
    {
        public static readonly Guid PHONE_BOOK_ACCESS_ID = new Guid("0000112f-0000-1000-8000-00805f9b34fb");

        public BluetoothPbapClientSession(BluetoothDevice bluetoothDevice) : base(bluetoothDevice, PHONE_BOOK_ACCESS_ID, ObexServiceUuid.PhonebookAccess)
        {
        }

        public override PbapClient CreateObexClient(StreamSocket socket)
        {
            return new PbapClient(socket.InputStream, socket.OutputStream);
        }
    }
}

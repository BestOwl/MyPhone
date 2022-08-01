using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using System;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.OBEX.Map
{
    public class BluetoothMnsServerSession : BluetoothObexServerSession<MnsServer>
    {
        public static readonly Guid MAP_MNS_Id = new Guid("00001133-0000-1000-8000-00805f9b34fb");

        public BluetoothMnsServerSession() : base(MAP_MNS_Id, 1)
        {
        }

        protected override MnsServer CreateObexServer(StreamSocket clientSocket)
        {
            return new MnsServer(clientSocket.InputStream, clientSocket.OutputStream);
        }
    }
}

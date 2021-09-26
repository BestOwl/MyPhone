using System;
using System.Collections.Generic;
using System.Text;
using Windows.Networking.Sockets;

namespace MyPhone.OBEX.Map
{
    public class BluetoothMnsServerSession : BluetoothObexServerSession<MnsServer>
    {
        public static readonly Guid MAP_MNS_Id = new Guid("00001133-0000-1000-8000-00805f9b34fb");

        public BluetoothMnsServerSession() : base(MAP_MNS_Id, 1)
        {
        }

        protected override MnsServer StartObexServer(StreamSocket clientSocket)
        {
            MnsServer mnsServer = new MnsServer(clientSocket.InputStream, clientSocket.OutputStream);
            mnsServer.StartServer();
            return mnsServer;
        }
    }
}

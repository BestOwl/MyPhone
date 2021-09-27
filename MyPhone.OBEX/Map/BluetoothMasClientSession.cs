using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace MyPhone.OBEX.Map
{
    public class BluetoothMasClientSession : BluetoothObexClientSession<MasClient>
    {
        public static readonly Guid MAP_Id = new Guid("00001132-0000-1000-8000-00805f9b34fb");

        public BluetoothMasClientSession(BluetoothDevice bluetoothDevice) : base(bluetoothDevice, MAP_Id, ObexServiceUuid.MessageAccess)
        {
        }

        protected override MasClient CreateObexClient(StreamSocket socket)
        {
            return new MasClient(socket.InputStream, socket.OutputStream);
        }
    }
}

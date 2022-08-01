using GoodTimeStudio.MyPhone.OBEX.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    internal class DumpBluetoothMasClientSession : BluetoothMasClientSession
    {
        public string DumpFileName { get; }
        private PcapDumpStream? _dumpStream;

        public DumpBluetoothMasClientSession(BluetoothDevice bluetoothDevice, string dumpFileName) : base(bluetoothDevice)
        {
            DumpFileName = dumpFileName;
        }

        protected override MasClient CreateObexClient(StreamSocket socket)
        {
            _dumpStream = new PcapDumpStream(socket.InputStream, socket.OutputStream, DumpFileName);
            return new MasClient(_dumpStream, _dumpStream);
        }
    }
}

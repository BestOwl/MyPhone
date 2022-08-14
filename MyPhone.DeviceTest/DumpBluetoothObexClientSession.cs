using GoodTimeStudio.MyPhone.OBEX;
using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class DumpBluetoothObexClientSession<TClient> : BluetoothObexClientSession<TClient> where TClient : ObexClient
    {
        private readonly BluetoothObexClientSession<TClient> _session;
        private PcapDumpStream? _dumpStream;

        public string DumpFileName { get; }

        public DumpBluetoothObexClientSession(BluetoothObexClientSession<TClient> session, string dumpFileName) : base(session.Device, session.ServiceUuid, session.TargetObexService)
        {
            _session = session;
            DumpFileName = dumpFileName;
        }

        public override TClient CreateObexClient(StreamSocket socket)
        {
            _dumpStream = new PcapDumpStream(socket.InputStream, socket.OutputStream, DumpFileName);
            return _session.CreateObexClient(socket);
        }

        public override void Dispose()
        {
            _dumpStream?.Dispose();
            base.Dispose();
        }
    }
}

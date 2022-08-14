using GoodTimeStudio.MyPhone.OBEX;
using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using Microsoft.Extensions.Configuration;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public abstract class BaseTestObexClient<TClient> : IAssemblyFixture<BluetoothDeviceFixture>, IAsyncLifetime where TClient : ObexClient
    {
        protected bool DumpPacket;

        protected string BluetoothDeviceId;

        protected BluetoothDevice Device = null!;

        protected BluetoothObexClientSession<TClient> Session = null!;

        public BaseTestObexClient(BluetoothDeviceFixture fixture)
        {
            BluetoothDeviceId = fixture.BluetoothDeviceId;
            DumpPacket = fixture.Configuration.GetValue("dumpObexPacket", false);
        }

        public async Task InitializeAsync()
        {
            Device = await BluetoothDevice.FromIdAsync(BluetoothDeviceId);
            Session = CreateSession();
            if (DumpPacket)
            {
                Session = new DumpBluetoothObexClientSession<TClient>(Session, $"{nameof(TClient)}-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.pcap");
            }
            await Session.ConnectAsync();
        }

        public abstract BluetoothObexClientSession<TClient> CreateSession();

        public Task DisposeAsync()
        {
            Session.Dispose();
            Device.Dispose();
            return Task.CompletedTask;
        }

        
    }
}

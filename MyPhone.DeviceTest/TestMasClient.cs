using GoodTimeStudio.MyPhone.OBEX.Map;
using Microsoft.Extensions.Configuration;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class TestMasClient : IAssemblyFixture<BluetoothDeviceFixture>, IAsyncLifetime
    {
        private BluetoothDeviceFixture _fixture;
        private BluetoothDevice? _deivce;
        private BluetoothMasClientSession? _session;

        public TestMasClient(BluetoothDeviceFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _deivce = await BluetoothDevice.FromIdAsync(_fixture.BluetoothDeviceId);
            if (_fixture.Configuration.GetValue("dumpObexPacket", false))
            {
                _session = new DumpBluetoothMasClientSession(_deivce,
                    $"MasClient-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.pcap");
            }
            else
            {
                _session = new BluetoothMasClientSession(_deivce);
            }
            await _session.ConnectAsync();
        }

        public Task DisposeAsync()
        {
            _session?.Dispose();
            _deivce?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task TestGetMessagesListingSizeAsync()
        {
            int count = await _session!.ObexClient!.GetMessageListingSizeAsync();
            Assert.True(count >= 0);
        }

        [SkippableFact]
        public async Task TestTraverseFolderAsync()
        {
            SmsFolder root = await _session!.ObexClient!.TraverseFolderAsync();
            Assert.Equal(1, root.Children.Count);
            SmsFolder telecom = root.Children[0];
            Assert.Equal(root, telecom.Parent);
            Assert.Equal("telecom", telecom.Name);

            // Some devices such as iPhone does not allow folder traversal, skip assertion
            Skip.If(telecom.Children.Count == 0, "Seems like your phone does not support folder traversal, is it iPhone?"); 
            Assert.Equal(1, telecom.Children.Count);

            SmsFolder msg = telecom.Children[0];
            Assert.Equal(telecom, msg.Parent);
            Assert.Equal("msg", msg.Name);
            Assert.True(msg.Children.Count >= 5);
            Assert.Contains(msg.Children, f => f.Name == "inbox" && f.Parent == msg);
            Assert.Contains(msg.Children, f => f.Name == "outbox" && f.Parent == msg);
            Assert.Contains(msg.Children, f => f.Name == "sent" && f.Parent == msg);
            Assert.Contains(msg.Children, f => f.Name == "deleted" && f.Parent == msg);
            Assert.Contains(msg.Children, f => f.Name == "draft" && f.Parent == msg);
        }
    }
}

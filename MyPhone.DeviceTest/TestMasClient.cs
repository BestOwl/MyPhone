using MyPhone.OBEX.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class TestMasClient : IAssemblyFixture<BluetoothDeviceFixture>, IAsyncLifetime
    {
        private BluetoothDeviceFixture _fixture;
        private BluetoothMasClientSession? _session;

        public TestMasClient(BluetoothDeviceFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _session = new BluetoothMasClientSession(_fixture.BluetoothDevice);
            await _session.ConnectAsync();
        }

        public Task DisposeAsync()
        {
            _session?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task TestGetMessagesListingAsync_GetListingSize()
        {
            await _session!.ObexClient!.GetMessagesListingAsync(0);
        }

        [Fact]
        public async Task TestTraverseFolderAsync()
        {
            SmsFolder root = await _session!.ObexClient!.TraverseFolderAsync();
            Assert.Equal(1, root.Children.Count);
            SmsFolder telecom = root.Children[0];
            Assert.Equal(root, telecom.Parent);
            Assert.Equal("telecom", telecom.Name);
            
            if (telecom.Children.Count != 0)
            {
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
            // else { } // iPhone does not allow folder traversal, skip assertion

        }
    }
}

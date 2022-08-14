using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using GoodTimeStudio.MyPhone.OBEX.Map;
using Microsoft.Extensions.Configuration;
using Windows.Devices.Bluetooth;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class TestMasClient : BaseTestObexClient<MasClient>
    {
        public TestMasClient(BluetoothDeviceFixture fixture) : base(fixture)
        {
        }

        public override BluetoothObexClientSession<MasClient> CreateSession()
        {
            return new BluetoothMasClientSession(Device);
        }

        [Fact]
        public async Task TestGetMessagesListingSizeAsync()
        {
            int count = await Session.ObexClient!.GetMessageListingSizeAsync();
            Assert.True(count >= 0);
        }

        [SkippableFact]
        public async Task TestTraverseFolderAsync()
        {
            SmsFolder root = await Session.ObexClient!.TraverseFolderAsync(false);
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

        [Fact]
        public async Task TestGetMessageListingAsync()
        {
            await Session.ObexClient!.GetMessagesListingAsync(0, 1024);
        }

        [Fact]
        public async Task TestGetAllMessagesAsync()
        {
            await Session.ObexClient!.SetFolderAsync(SetPathMode.EnterFolder, "telecom");
            await Session.ObexClient!.SetFolderAsync(SetPathMode.EnterFolder, "msg");
            List<string> handles = await Session.ObexClient!.GetAllMessagesAsync("inbox");
            Assert.True(handles.Count > 0);
        }

        
    }
}

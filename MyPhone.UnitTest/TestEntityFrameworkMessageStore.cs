using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.Device.Services;
using GoodTimeStudio.MyPhone.Extensions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyPhone.UnitTest
{
    public class TestEntityFrameworkMessageStore : IDisposable
    {
        private readonly DeviceDbContext _context;
        private readonly EntityFrameworkMessageStore _messageStore;

        public TestEntityFrameworkMessageStore(DeviceDbContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _context.Messages.RemoveRange(_context.Messages);
            _messageStore = new EntityFrameworkMessageStore(context);
        }

        [Fact]
        public async Task TestSaveMessage()
        {
            Message message = new Message
            {
                MessageHandle = "1",
                Status = MessageStatus.Unread,
                Type = "SMS/MNS",
                Folder = "/telecom/msg/inbox",
                Sender = new MixERP.Net.VCards.VCard
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                }.ToContact(),
                Body = "Hello world"
            };
            await _messageStore.SaveMessageAsync(message);
        }

        [Fact]
        public async Task TestGetMessage()
        {
            Assert.Null(await _messageStore.GetMessageAsync("10"));

            Message message = new Message
            {
                MessageHandle = "2022",
                Status = MessageStatus.Unread,
                Type = "SMS/MNS",
                Folder = "/telecom/msg/inbox",
                Sender = new MixERP.Net.VCards.VCard
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                }.ToContact(),
                Body = "Hello world"
            };
            await _messageStore.SaveMessageAsync(message);

            Message? msg = await _messageStore.GetMessageAsync("2022");
            Assert.NotNull(msg);
            Assert.Equal(message, msg);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

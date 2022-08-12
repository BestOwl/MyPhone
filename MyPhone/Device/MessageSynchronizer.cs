using GoodTimeStudio.MyPhone.Data;
using GoodTimeStudio.MyPhone.OBEX;
using GoodTimeStudio.MyPhone.OBEX.Map;
using GoodTimeStudio.MyPhone.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Device
{
    public class MessageSynchronizer
    {
        private readonly MasClient _masClient;
        private readonly IMessageStore _messageStore;
        private readonly ILogger<MessageSynchronizer> _logger;

        private List<string>? _pendingSyncHandles;

        public bool Syncing { get; private set; }

        public MessageSynchronizer(MasClient masClient, IMessageStore messageStore, ILogger<MessageSynchronizer> logger)
        {
            _masClient = masClient ?? throw new ArgumentNullException(nameof(masClient));
            _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
            _logger = logger;
        }

        public async Task SyncMessagesAsync()
        {
            if (Syncing)
            {
                throw new InvalidOperationException("Syncing is already in progress.");
            }
            Syncing = true;
            _logger.LogInformation("Starting to sync messages");

            try
            {
                _logger.LogInformation("Entering folder: /telecom/msg/");
                await _masClient.SetFolderAsync(SetPathMode.BackToRoot);
                await _masClient.SetFolderAsync(SetPathMode.EnterFolder, "telecom");
                await _masClient.SetFolderAsync(SetPathMode.EnterFolder, "msg");
                
                _logger.LogInformation("Current folder: /telecom/msg/");
                _logger.LogInformation("Syncing messages in inbox");
                _pendingSyncHandles = await _masClient.GetAllMessagesAsync("inbox");
                _logger.LogInformation("Checking {HandleCount} message handles.", _pendingSyncHandles.Count);
                foreach (string messageHandle in _pendingSyncHandles)
                {
                    if (!await _messageStore.Contains(messageHandle))
                    {
                        _logger.LogTrace("Message handle {MessageHandle} is not in db, retrieving.", messageHandle);
                        BMessage message = await _masClient.GetMessageAsync(messageHandle);
                        await _messageStore.SaveMessageAsync(Message.FromBMessage(messageHandle, message));
                    }
                }
                _logger.LogInformation("Folder /telecom/msg/inbox is synced");
            }
            finally
            {
                Syncing = false;
            }
        }
    }
}

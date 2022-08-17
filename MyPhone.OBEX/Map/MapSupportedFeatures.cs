using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.OBEX.Map
{
    [Flags]
    public enum MapSupportedFeatures
    {
        None = 0,
        NotificationRegistration = 1,
        Notification = 2,
        Browsing = 4,
        Uploading = 8,
        Delete = 16,
        InstanceInformation = 32,
        ExtendedEventReport_v1_1 = 64,
        EventReport_v1_2 = 128,
        MessageFormat_v1_1 = 256,
        MessagesListingFormat_v1_1 = 512,
        PersistentMessageHandles = 1024,
        DatabaseIdentifier = 2048,
        FolderVersionCounter = 4096,
        ConversationVersionCounters = 8192,
        ParticipantPresenceChangeNotification = 16384,
        ParticipantChatStateChangeNotification = 32768,
        PbapContactCrossReference = 65536,
        NotificationFiltering = 131072,
        UtcOffsetTimestampFormat = 262144,
        MapSupportedFeaturesInConnectRequest = 524288,
        ConversationListing = 1048576,
        OwnerStatus = 2097152,
        MessageForwarding = 4194304
        // As of MAP v1.4.2
    }
}

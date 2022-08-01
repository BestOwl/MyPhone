using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Map
{
    public enum MasAppParamTagId : byte
    {
        MaxListCount = 0x01,
        ListStartOffset,
        FilterMessageType,
        FilterPeriodBegin,
        EndFilterPeriodEnd,
        FilterReadStatus,
        FilterRecipient,
        FilterOriginator,
        FilterPriority,
        Attachment,
        Transparent,
        Retry,
        NewMessage,
        NotificationStatus,
        MASInstanceID,
        ParameterMask,
        FolderListingSize,
        ListingSize,
        SubjectLength,
        Charset,
        FractionRequest,
        FractionDeliver,
        StatusIndicator,
        StatusValue,
        MSETime,
        DatabaseIdentifier,
        ConversationListingVersionCounter,
        PresenceAvailability,
        PresenceText,
        LastActivity,
        FilterLastActivityBegin,
        FilterLastActivityEnd,
        ChatState,
        ConversationID,
        FolderVersionCounter,
        FilterMessageHandle,
        NotificationFilterMask,
        ConvParameterMask,
        OwnerUCI,
        ExtendedData,
        MapSupportedFeatures,
        MessageHandle,
        ModifyText
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MyPhone.OBEX
{
    public class AppParamHeader : OBEXHeader, ILengthRequiredHeader
    {
        private static readonly int TAG_LEN_BITS = 2;

        public AppParamTagId TagId { get; set; }

        public byte[] Value;

        public AppParamHeader(AppParamTagId tagId) : base(HeaderId.ApplicationParameters) 
        {
            TagId = tagId;
        }

        /// <summary>
        /// Construct a UTF-8 string valued app param header
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="value"></param>
        public AppParamHeader(AppParamTagId tagId, string value) : this(tagId)
        {
            int count = Encoding.UTF8.GetByteCount(value);
            if (count > 128)
            {
                throw new NotSupportedException("String more that 126 bytes is not allowed");
            }
            Value = new byte[TAG_LEN_BITS + count + 1];
            Value[0] = (byte)TagId;
            Value[1] = (byte) count;
            Encoding.UTF8.GetBytes(value, Value.AsSpan().Slice(TAG_LEN_BITS));
            Value[count] = 0; // null terminator
        }

        public AppParamHeader(AppParamTagId tagId, byte[] value) : this(tagId)
        {
            Value = new byte[value.Length + TAG_LEN_BITS];
            if (value.Length > 128)
            {
                throw new NotSupportedException("Array more that 126 bytes is not allowed");
            }
            Value[0] = (byte) TagId;
            Value[1] = (byte) value.Length;
            Array.Copy(value, 0, Value, 2, value.Length);
        }

        public AppParamHeader(AppParamTagId tagId, byte value) : this(tagId, new byte[] { value }) { }

        public ushort GetValueLength()
        {
            return (ushort)(Value.Length);
        }

        public override byte[] ToBytes()
        {
            return Value;
        }
    }

    public enum AppParamTagId : byte
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

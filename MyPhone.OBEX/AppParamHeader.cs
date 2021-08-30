using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class AppParamHeader : IOBEXHeader
    {

        public HeaderId HeaderId => HeaderId.ApplicationParameters;

        public LinkedList<AppParameter> AppParameters;

        public AppParamHeader() 
        {
            AppParameters = new LinkedList<AppParameter>();
        }

        public AppParamHeader(params AppParameter[] parameters) : this()
        {
            foreach (AppParameter param in parameters)
            {
                AppParameters.AddLast(param);
            }
        }

        public byte[] ToBytes()
        {
            DataWriter writer = new DataWriter();
            foreach (AppParameter param in AppParameters)
            {
                param.ToBytes(writer);
            }
            return writer.DetachBuffer().ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; )
            {
                AppParameter param = new AppParameter((AppParamTagId)bytes[i++]);
                param.Length = bytes[i++];
                param.Content = new byte[param.Length];
                Array.Copy(bytes, i, param.Content, 0, param.Length);
                i += param.Length;
                AppParameters.AddLast(param);
            }
        }

        public ushort GetFixedLength()
        {
            return 0;
        }

        public byte [] RawBytes { get; set; }
    }

    public class AppParameter
    {
        private static readonly byte _MAX_LEN = 255;

        public AppParamTagId TagId { get; set; }

        public byte Length { get; set; }

        public byte[] Content;

        public AppParameter(AppParamTagId tagId)
        {
            TagId = tagId;
        }

        public AppParameter(AppParamTagId tagId, string text) : this(tagId)
        {
            int count = Encoding.UTF8.GetByteCount(text);
            if (count > _MAX_LEN)
            {
                throw new NotSupportedException("String more that 126 bytes is not allowed");
            }
            Content = Encoding.UTF8.GetBytes(text);
        }

        public AppParameter(AppParamTagId tagId, byte[] value) : this(tagId)
        {
            if (value.Length > 128)
            {
                throw new NotSupportedException("Array more that 126 bytes is not allowed");
            }
            Content = value;
        }

        public AppParameter(AppParamTagId tagId, byte value) : this(tagId, new byte[] { value }) { }

        public void ToBytes(DataWriter writer)
        {
            writer.WriteByte((byte)TagId);
            writer.WriteByte((byte)Content.Length);
            writer.WriteBytes(Content);
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

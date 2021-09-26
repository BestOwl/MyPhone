using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public class AppParamHeader : IObexHeader
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
                AppParamTagId tagId = (AppParamTagId)bytes[i++];
                byte len = bytes[i++];
                byte[] buf = new byte[len];
                Array.Copy(bytes, i, buf, 0, len);
                i += len;

                AppParameter param = new AppParameter(tagId, len);
                AppParameters.AddLast(param);
            }
        }

        public ushort GetFixedLength()
        {
            return 0;
        }

    }

    public class AppParameter
    {
        private static readonly byte _MAX_LEN = byte.MaxValue - sizeof(byte) - sizeof(byte);

        public AppParamTagId TagId { get; set; }

        public byte Length { get; set; }

        public byte[] Content;

        public AppParameter(AppParamTagId tagId, string text)
        {
            TagId = tagId;
            int count = Encoding.UTF8.GetByteCount(text);
            if (count > _MAX_LEN)
            {
                throw new NotSupportedException("String more that 126 bytes is not allowed");
            }
            Content = Encoding.UTF8.GetBytes(text);
            Length = (byte) count;
        }

        public AppParameter(AppParamTagId tagId, byte[] value)
        {
            TagId = tagId;
            if (value.Length > _MAX_LEN)
            {
                throw new NotSupportedException($"Array more that {_MAX_LEN} bytes is not allowed");
            }
            Content = value;
            Length = (byte) value.Length;
        }

        public AppParameter(AppParamTagId tagId, byte value) : this(tagId, new byte[] { value }) { }

        public AppParameter(AppParamTagId tagId, ushort value)
        {
            TagId = tagId;
            Length = sizeof(ushort);
            Content = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(Content);
            }
        }

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

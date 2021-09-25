using System;
using System.Collections.Generic;
using System.Text;

namespace MyPhone.OBEX
{
    public class BMessage
    {

        public MessageStatus Status { get; set; }

        public string? Type { get; set; }

        public string? Folder { get; set; }

        // TODO: implement vCard
        public string? Sender { get; set; }

        public string? Charset { get; set; }

        public int Length { get; set; }

        public string? Body { get; set; }
    }

    public enum MessageStatus
    {
        UNREAD,
        READ
    }
}

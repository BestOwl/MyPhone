using MixERP.Net.VCards;
using System;

namespace GoodTimeStudio.MyPhone.OBEX
{
    public class BMessage
    {

        public BMessageStatus Status { get; set; }

        public string Type { get; set; }

        public string Folder { get; set; }

        public VCard Sender { get; set; }

        public string Charset { get; set; }

        public int Length { get; set; }

        public string Body { get; set; }

        public BMessage(BMessageStatus status, string type, string folder, VCard sender, string charset, int length, string body)
        {
            Status = status;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Charset = charset ?? throw new ArgumentNullException(nameof(charset));
            Length = length;
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }

    public enum BMessageStatus
    {
        UNREAD,
        READ
    }
}

using GoodTimeStudio.MyPhone.Extensions;
using GoodTimeStudio.MyPhone.OBEX;
using MixERP.Net.VCards;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Data
{
    public class Message
    {
        [Key]
        public string MessageHandle { get; set; } = null!;

        public MessageStatus Status { get; set; }

        public string Type { get; set; } = null!;

        public string Folder { get; set; } = null!;

        public Contact Sender { get; set; } = null!;

        public string Body { get; set; } = null!;

        public static Message FromBMessage(string messageHandle, BMessage b)
        {
            return new Message
            {
                MessageHandle = messageHandle,
                Body = b.Body,
                Status = b.Status == BMessageStatus.READ ? MessageStatus.Read : MessageStatus.Unread,
                Folder = b.Folder,
                Sender = b.Sender.ToContact(),
                Type = b.Type
            };
        }
    }

    public enum MessageStatus
    {
        Unread,
        Read
    }
}

using MixERP.Net.VCards;
using MyPhone.OBEX;
using System.Collections.Generic;

namespace GoodTimeStudio.MyPhone.Models
{
    public class Conversation
    {
        public VCard ContactInfo { get; set; }

        public IList<BMessage> Messages { get; set; }
    }
}

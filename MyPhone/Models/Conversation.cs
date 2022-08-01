using GoodTimeStudio.MyPhone.OBEX;
using MixERP.Net.VCards;
using System.Collections.Generic;

namespace GoodTimeStudio.MyPhone.Models
{
    public class Conversation
    {
        public VCard ContactInfo { get; set; }

        public IList<BMessage> Messages { get; set; }
    }
}

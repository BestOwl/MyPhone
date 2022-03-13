using MixERP.Net.VCards;
using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Models
{
    public class Conversation
    {
        public VCard ContactInfo { get; set; }

        public IList<BMessage> Messages { get; set; }
    }
}

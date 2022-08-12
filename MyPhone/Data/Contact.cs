using MixERP.Net.VCards;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Data
{
    public class Contact
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public string MiddleName { get; set; } = string.Empty;

        public string NickName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FormattedName { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;

        public string OrganizationalUnit { get; set; } = string.Empty;

        public VCard Detail { get; set; } = null!;
    }
}

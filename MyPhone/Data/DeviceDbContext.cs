using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MixERP.Net.VCards;
using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Data
{
    public class DeviceDbContext : DbContext
    {
        public DbSet<BMessage> Messages => Set<BMessage>();
        public DbSet<VCard> Contacts => Set<VCard>();

        public DeviceDbContext(DbContextOptions options) : base(options) { }
    }
}

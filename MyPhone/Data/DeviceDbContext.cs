using GoodTimeStudio.MyPhone.OBEX;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using MixERP.Net.VCards;
using MixERP.Net.VCards.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Data
{
    public class DeviceDbContext : DbContext
    {
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<DeviceConfiguration> Configurations => Set<DeviceConfiguration>();

        public DeviceDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder
                .Entity<Contact>()
                .Property(c => c.Detail)
                .HasConversion(new ValueConverter<VCard, string>(
                    v => v.Serialize(),
                    v => Deserializer.GetVCard(v)));

        }
    }
}

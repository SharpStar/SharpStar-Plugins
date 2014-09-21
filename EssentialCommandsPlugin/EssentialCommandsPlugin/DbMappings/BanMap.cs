using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class BanMap : ClassMap<Ban>
    {
        public BanMap()
        {
            Id(m => m.Id);
            Map(m => m.IPAddress);
            Map(m => m.BanReason);
            Map(m => m.ExpirationTime).Nullable();
            Map(m => m.UserAccountId).Nullable();
            HasMany(m => m.BanUUIDs).Cascade.AllDeleteOrphan().KeyColumn("BanId").Inverse();
        }
    }
}

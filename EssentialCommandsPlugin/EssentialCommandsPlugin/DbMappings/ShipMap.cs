using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class ShipMap : ClassMap<Ship>
    {
        public ShipMap()
        {
            Id(m => m.Id);
            Map(m => m.OwnerUserAccountId).Not.Nullable();
            Map(m => m.Public);
            HasMany(m => m.ShipUsers).LazyLoad().Cascade.All();
        }
    }
}

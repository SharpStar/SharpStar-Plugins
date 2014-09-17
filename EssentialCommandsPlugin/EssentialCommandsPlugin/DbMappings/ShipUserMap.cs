using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class ShipUserMap : ClassMap<ShipUser>
    {
        public ShipUserMap()
        {
            Id(m => m.Id);
            Map(m => m.UserAccountId);
            References(m => m.Ship).LazyLoad().Not.Nullable();
            Map(m => m.HasAccess);
        }
    }
}

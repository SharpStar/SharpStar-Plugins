using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class BanUUIDMap : ClassMap<BanUUID>
    {
        public BanUUIDMap()
        {
            Id(m => m.Id);
            References(m => m.Ban).LazyLoad().Not.Nullable();
            Map(m => m.PlayerName);
            Map(m => m.UUID);
        }
    }
}

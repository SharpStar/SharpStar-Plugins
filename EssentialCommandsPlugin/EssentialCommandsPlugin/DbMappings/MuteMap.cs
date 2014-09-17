using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class MuteMap : ClassMap<Mute>
    {
        public MuteMap()
        {
            Id(m => m.ID);
            Map(m => m.UserId).Not.Nullable();
            Map(m => m.ExpireTime).Nullable();
        }
    }
}

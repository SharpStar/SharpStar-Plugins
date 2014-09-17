using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class BanIPMap : ClassMap<BanIP>
    {
        public BanIPMap()
        {
            Id(m => m.Id);
            Map(m => m.IPAddress);
        }
    }
}

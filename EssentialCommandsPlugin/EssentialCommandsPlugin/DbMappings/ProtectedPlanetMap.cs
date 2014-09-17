using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class ProtectedPlanetMap : ClassMap<ProtectedPlanet>
    {
        public ProtectedPlanetMap()
        {
            Id(m => m.ID);
            Map(m => m.OwnerId).Not.Nullable();
            Map(m => m.Sector);
            Map(m => m.X);
            Map(m => m.Y);
            Map(m => m.Z);
            Map(m => m.Planet);
            Map(m => m.Satellite);
            HasMany(m => m.Builders).LazyLoad().Cascade.All();
        }
    }
}

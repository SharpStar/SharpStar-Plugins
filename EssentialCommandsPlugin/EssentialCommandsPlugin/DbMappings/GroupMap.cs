using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class GroupMap : ClassMap<Group>
    {
        public GroupMap()
        {
            Id(m => m.Id);
            Map(m => m.GroupId).Not.Nullable();
            Map(m => m.Prefix);
            Map(m => m.ProtectedPlanetLimit).Nullable();
            HasMany(m => m.Commands).LazyLoad().Cascade.All().KeyColumn("GroupId").Inverse();
        }
    }
}

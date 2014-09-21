using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class CommandMap : ClassMap<Command>
    {
        public CommandMap()
        {
            Id(m => m.Id);
            Map(m => m.GroupId).Nullable();
            References(m => m.Group).LazyLoad().Nullable().Column("GroupId");
            Map(m => m.CommandName);
            Map(m => m.CommandLimit).Nullable();
            HasMany(m => m.UserCommands).LazyLoad().Cascade.AllDeleteOrphan().KeyColumn("CommandId").Inverse();
        }
    }
}

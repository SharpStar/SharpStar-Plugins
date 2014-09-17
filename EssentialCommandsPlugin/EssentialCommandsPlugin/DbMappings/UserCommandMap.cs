using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using FluentNHibernate.Mapping;

namespace EssentialCommandsPlugin.DbMappings
{
    public class UserCommandMap : ClassMap<UserCommand>
    {
        public UserCommandMap()
        {
            Id(m => m.Id);
            Map(m => m.UserId).Not.Nullable();
            References(m => m.Command).LazyLoad().Not.Nullable();
            Map(m => m.TimesUsed);
        }
    }
}

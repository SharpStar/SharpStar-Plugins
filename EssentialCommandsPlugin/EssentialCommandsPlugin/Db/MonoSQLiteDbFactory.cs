using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Runner.Processors;

namespace EssentialCommandsPlugin.Db
{
    public class MonoSQLiteDbFactory : ReflectionBasedDbFactory
    {
        public MonoSQLiteDbFactory()
            : base("Mono.Data.Sqlite", "Mono.Data.Sqlite.SqliteFactory")
        {
        }
    }
}

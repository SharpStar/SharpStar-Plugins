using System;
using System.Collections.Generic;
using System.Reflection;
using EssentialCommandsPlugin.Db;
using EssentialCommandsPlugin.DbMappings;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Mono;
using FluentMigrator;

namespace EssentialCommandsPlugin.DbModels
{

    public static class EssentialsDb
    {

        public const string DatabaseFile = "EssentialsDb.db";

        private static readonly ISessionFactory Factory;

        private static Configuration _config;

        static EssentialsDb()
        {
            Factory = GetSessionFactory();
        }

        public static ISessionFactory GetSessionFactory()
        {
            var config = Fluently.Configure();

            if (MonoHelper.IsRunningOnMono())
            {
                config = config.Database(MonoSQLiteConfiguration.Standard.UsingFile(DatabaseFile));
            }
            else
            {
                config = config.Database(SQLiteConfiguration.Standard.UsingFile(DatabaseFile));
            }

            Assembly thisAssm = Assembly.GetCallingAssembly();

            _config = config
              .Mappings(p => p.FluentMappings.AddFromAssembly(thisAssm))
              .ExposeConfiguration(p => new SchemaUpdate(p).Execute(false, true)).BuildConfiguration();

            MigrateToLatest();

            return config.BuildSessionFactory();
        }

        public static ISession CreateSession()
        {
            return Factory.OpenSession();
        }

        public class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }
            public string ProviderSwitches { get; set; }
            public int Timeout { get; set; }
        }

        public static void MigrateToLatest()
        {
            var announcer = new TextWriterAnnouncer(s => EssentialCommands.Logger.Debug(s));
            var assembly = Assembly.GetCallingAssembly();

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = "EssentialCommandsPlugin.DbMigrations"
            };

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };

            ReflectionBasedDbFactory factory;

            if (MonoHelper.IsRunningOnMono())
                factory = new MonoSQLiteDbFactory();
            else
                factory = new SqliteDbFactory();

            var connection = factory.CreateConnection(_config.GetProperty(NHibernate.Cfg.Environment.ConnectionString));

            var processor = new SqliteProcessor(connection, new SqliteGenerator(), announcer, options, factory);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runner.MigrateUp(true);
        }
    }

    public class Ban
    {
        public virtual int Id { get; set; }

        public virtual string IPAddress { get; set; }

        public virtual string BanReason { get; set; }

        public virtual DateTime? ExpirationTime { get; set; }

        public virtual int? UserAccountId { get; set; }

        public virtual IList<BanUUID> BanUUIDs { get; set; }

        public Ban()
        {
            BanUUIDs = new List<BanUUID>();
        }
    }

    public class BanUUID
    {
        public virtual int Id { get; set; }

        public virtual Ban Ban { get; set; }

        public virtual string PlayerName { get; set; }

        public virtual string UUID { get; set; }
    }

    public class BanIP
    {
        public virtual int Id { get; set; }

        public virtual string IPAddress { get; set; }
    }

    public class Ship
    {
        public virtual int Id { get; set; }

        public virtual int OwnerUserAccountId { get; set; }

        public virtual bool Public { get; set; }

        public virtual IList<ShipUser> ShipUsers { get; set; }

        public Ship()
        {
            ShipUsers = new List<ShipUser>();
        }
    }

    public class ShipUser
    {
        public virtual int Id { get; set; }

        public virtual int UserAccountId { get; set; }

        public virtual Ship Ship { get; set; }

        public virtual bool HasAccess { get; set; }

    }

    public class Mute
    {
        public virtual int ID { get; set; }

        public virtual int UserId { get; set; }

        public virtual DateTime? ExpireTime { get; set; }
    }

    public class ProtectedPlanet
    {
        public virtual int ID { get; set; }

        public virtual int OwnerId { get; set; }

        public virtual string Sector { get; set; }

        public virtual int X { get; set; }

        public virtual int Y { get; set; }

        public virtual int Z { get; set; }

        public virtual int Planet { get; set; }

        public virtual int Satellite { get; set; }

        public virtual IList<Builder> Builders { get; set; }

        public ProtectedPlanet()
        {
            Builders = new List<Builder>();
        }
    }

    public class Builder
    {
        public virtual int Id { get; set; }

        public virtual int UserId { get; set; }

        public virtual ProtectedPlanet ProtectedPlanet { get; set; }

        public virtual bool Allowed { get; set; }
    }

    public class Group
    {
        public virtual int Id { get; set; }

        public virtual int GroupId { get; set; }

        public virtual string Prefix { get; set; }

        public virtual int? ProtectedPlanetLimit { get; set; }

        public virtual IList<Command> Commands { get; set; }

        public Group()
        {
            Commands = new List<Command>();
        }
    }

    public class Command
    {
        public virtual int Id { get; set; }

        public virtual int GroupId { get; set; }

        public virtual Group Group { get; set; }

        public virtual string CommandName { get; set; }

        public virtual int? CommandLimit { get; set; }

        public virtual IList<UserCommand> UserCommands { get; set; }
    }

    public class UserCommand
    {
        public virtual int Id { get; set; }

        public virtual int UserId { get; set; }

        public virtual Command Command { get; set; }

        public virtual int TimesUsed { get; set; }
    }

}

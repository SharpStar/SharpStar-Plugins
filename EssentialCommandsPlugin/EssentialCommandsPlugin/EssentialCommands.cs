using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EssentialCommandsPlugin.Commands;
using EssentialCommandsPlugin.ConsoleCommands;
using EssentialCommandsPlugin.DbModels;
using Mono.Addins;
using NHibernate.Linq;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;
using SQLite;

[assembly: Addin("EssentialCommands", Version = "1.0.6.8")]
[assembly: AddinDescription("A command plugin that is essential")]
[assembly: AddinProperty("sharpstar", "0.2.4.1")]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]
[assembly: ImportAddinAssembly("NHibernate.dll")]
[assembly: ImportAddinAssembly("FluentMigrator.dll")]
[assembly: ImportAddinAssembly("FluentMigrator.Runner.dll")]
[assembly: ImportAddinAssembly("Iesi.Collections.dll")]
[assembly: ImportAddinAssembly("FluentNHibernate.dll")]
namespace EssentialCommandsPlugin
{
    [Extension]
    public class EssentialCommands : CSPlugin
    {

        public static EssentialCommandsConfig Config;

        private const string DatabaseName = "essentialcommands.db";

        private const string ConfigFileName = "essentialcommands.json";

        //public static readonly EssentialCommandsDb Database = new EssentialCommandsDb(DatabaseName);

        public static readonly SharpStarLogger Logger = new SharpStarLogger("Essentials");

        #region Commands

        private readonly MakeRemoveAdminCommand _makeRemoveAdmin = new MakeRemoveAdminCommand();
        private readonly KickCommand _kickCommand = new KickCommand();
        private readonly BanCommand _banCommand = new BanCommand();
        private readonly BroadcastCommand _broadcastCommand = new BroadcastCommand();
        private readonly GiveItemCommand _giveItemCommand = new GiveItemCommand();
        private readonly PermissionCommands _permCommands = new PermissionCommands();
        private readonly MotdCommand _motdCommands = new MotdCommand();
        private readonly WarpCommands _warpCommands = new WarpCommands();
        private readonly KillCommand _killCommand = new KillCommand();
        private readonly AdvertCommands _advertCommands = new AdvertCommands();
        private readonly WhoCommands _whoCommands = new WhoCommands();
        private readonly ShipCommand _shipCommand = new ShipCommand();
        private readonly MuteCommand _muteCommand = new MuteCommand();
        private readonly SpawnCommands _spawnCommands = new SpawnCommands();
        private readonly ProtectPlanetCommands _planetProtect = new ProtectPlanetCommands();
        private readonly HelpCommand _helpCommand = new HelpCommand();

        #endregion

        #region Console Commands

        private readonly HelpConsoleCommand _helpConsoleCommand = new HelpConsoleCommand();

        #endregion

        public override string Name
        {
            get { return "Essential Commands"; }
        }


        static EssentialCommands()
        {
            Config = new EssentialCommandsConfig(ConfigFileName);
        }

        public override void OnLoad()
        {
            Config = new EssentialCommandsConfig(ConfigFileName);
            Config.Save();

            MigrateDb();

            RegisterCommandObject(_makeRemoveAdmin);
            RegisterCommandObject(_kickCommand);
            RegisterCommandObject(_banCommand);
            RegisterCommandObject(_broadcastCommand);
            RegisterCommandObject(_giveItemCommand);
            RegisterCommandObject(_permCommands);
            RegisterCommandObject(_motdCommands);
            RegisterCommandObject(_warpCommands);
            RegisterCommandObject(_killCommand);
            RegisterCommandObject(_advertCommands);
            RegisterCommandObject(_whoCommands);
            RegisterCommandObject(_shipCommand);
            RegisterCommandObject(_muteCommand);
            RegisterCommandObject(_spawnCommands);
            RegisterCommandObject(_planetProtect);
            RegisterCommandObject(_helpCommand);

            RegisterConsoleCommandObject(_helpConsoleCommand);

            RegisterEventObject(_banCommand);
            RegisterEventObject(_motdCommands);
            RegisterEventObject(_shipCommand);
            RegisterEventObject(_muteCommand);
            RegisterEventObject(_planetProtect);
            RegisterEventObject(_permCommands);

            _advertCommands.StartSendingAdverts();
        }

        public override void OnUnload()
        {
            _advertCommands.StopSendingAdverts();

            foreach (var cmdObj in RegisteredCommandObjects)
            {
                if (cmdObj.Key is IDisposable)
                {
                    ((IDisposable)cmdObj.Key).Dispose();
                }
            }

        }

        private void MigrateDb()
        {
            if (File.Exists(DatabaseName))
            {
                Logger.Info("Migrating Database...");

                using (var session = EssentialsDb.CreateSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        var conn = new SQLiteConnection(DatabaseName);

                        foreach (var ban in conn.Table<EssentialCommandsBan>())
                        {
                            Ban newBan = new Ban
                            {
                                BanReason = ban.BanReason,
                                ExpirationTime = ban.ExpirationTime,
                                UserAccountId = ban.UserAccountId
                            };

                            int banId = ban.Id;
                            foreach (var uuidBan in conn.Table<EssentialCommandsBanUUID>().Where(p => p.BanId == banId))
                            {
                                BanUUID banUuid = new BanUUID
                                {
                                    PlayerName = uuidBan.PlayerName,
                                    UUID = uuidBan.UUID,
                                    Ban = newBan
                                };

                                newBan.BanUUIDs.Add(banUuid);
                            }

                            session.Save(newBan);
                        }

                        foreach (var ship in conn.Table<EssentialCommandsShip>())
                        {

                            Ship newShip = new Ship
                            {
                                OwnerUserAccountId = ship.OwnerUserAccountId,
                                Public = ship.Public
                            };

                            int shipId = ship.Id;
                            foreach (var shipUser in conn.Table<EssentialCommandsShipUser>().Where(p => p.ShipId == shipId))
                            {
                                ShipUser newShipUser = new ShipUser
                                {
                                    Ship = newShip,
                                    UserAccountId = shipUser.UserAccountId,
                                    HasAccess = shipUser.HasAccess
                                };

                                newShip.ShipUsers.Add(newShipUser);
                            }

                            session.Save(newShip);
                        }

                        foreach (var mute in conn.Table<EssentialCommandsMute>())
                        {
                            Mute newMute = new Mute
                            {
                                ExpireTime = mute.ExpireTime,
                                UserId = mute.UserId,
                            };

                            session.Save(newMute);
                        }

                        foreach (var planet in conn.Table<EssentialCommandsPlanet>())
                        {
                            ProtectedPlanet newPlanet = new ProtectedPlanet
                            {
                                OwnerId = planet.OwnerId,
                                Planet = planet.Planet,
                                Satellite = planet.Satellite,
                                Sector = planet.Sector,
                                X = planet.X,
                                Y = planet.Y,
                                Z = planet.Z
                            };

                            int planetId = planet.Id;
                            foreach (var builder in conn.Table<EssentialCommandsBuilder>().Where(p => p.PlanetId == planetId))
                            {
                                Builder newBuilder = new Builder
                                {
                                    ProtectedPlanet = newPlanet,
                                    UserId = builder.UserId,
                                    Allowed = builder.Allowed
                                };

                                newPlanet.Builders.Add(newBuilder);
                            }

                            session.Save(newPlanet);
                        }

                        foreach (var group in conn.Table<EssentialCommandsGroup>())
                        {
                            Group newGroup = new Group
                            {
                                Prefix = group.Prefix,
                                ProtectedPlanetLimit = group.ProtectedPlanetLimit,
                                GroupId = group.Id
                            };

                            session.Save(newGroup);
                        }

                        foreach (var command in conn.Table<EssentialCommandsCommand>())
                        {
                            var newCommand = new Command
                            {
                                CommandName = command.Command,
                                GroupId = command.GroupId,
                                CommandLimit = command.Limit
                            };

                            int commandId = command.Id;
                            foreach (var userCommand in conn.Table<EssentialCommandsUserCommand>().Where(p => p.CommandId == commandId))
                            {
                                UserCommand newUserCommand = new UserCommand
                                {
                                    Command = newCommand,
                                    TimesUsed = userCommand.TimesUsed,
                                    UserId = userCommand.UserId
                                };

                                session.Save(newUserCommand);
                            }

                            session.Save(newCommand);
                        }

                        transaction.Commit();

                        Logger.Info("Migration Complete!");

                        conn.Close();
                        conn.Dispose();

                        File.Move(DatabaseName, DatabaseName + ".old");
                    }
                }
            }
        }

        public override Task<bool> OnChatCommandReceived(SharpStarClient client, string command, string[] args)
        {

            if (client.Server.Player != null && client.Server.Player.UserGroupId.HasValue && client.Server.Player.UserAccount != null)
            {

                using (var session = EssentialsDb.CreateSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        Command cmd = session.Query<Command>().SingleOrDefault(p => p.GroupId == client.Server.Player.UserGroupId.Value);

                        if (cmd != null)
                        {
                            UserCommand uc = session.Query<UserCommand>().SingleOrDefault(p => p.UserId == client.Server.Player.UserAccount.Id);

                            if (uc != null)
                            {
                                uc.TimesUsed++;

                                if (cmd.CommandLimit < uc.TimesUsed)
                                {
                                    client.SendChatMessage("Server", "You have reached the limit for this command!");

                                    return Task.FromResult(true);
                                }
                            }

                            session.SaveOrUpdate(uc);

                            transaction.Commit();
                        }
                    }

                }

            }

            return base.OnChatCommandReceived(client, command, args);
        }

        public static async Task KickBanPlayer(SharpStarServerClient kickBanner, List<SharpStarServerClient> players, bool ban = false, string banReason = "", DateTime? expireTime = null)
        {

            for (int i = 0; i < players.Count; i++)
            {

                var plr = players[i];

                if (!plr.ServerClient.Connected)
                    continue;

                if (!ban)
                    plr.PlayerClient.SendChatMessage("Server", "You are being kicked. Goodbye.");
                else
                    plr.PlayerClient.SendChatMessage("Server", "You have been banned. Goodbye.");

                await Task.Factory.StartNew(() =>
                {

                    if (!ban)
                    {
                        plr.ServerClient.ClientDisconnected += (sender, e) =>
                        {
                            if (kickBanner.PlayerClient.Connected)
                                kickBanner.PlayerClient.SendChatMessage("Server", String.Format("Player {0} has been kicked!", plr.Player.Name));

                            Logger.Info("Player {0} has kicked by {1} ({2})", e.Client.Server.Player.Name, kickBanner.Player.Name, kickBanner.Player.UserAccount.Username);
                        };
                    }
                    else
                    {

                        int? acctId = null;

                        if (plr.Player.UserAccount != null)
                            acctId = plr.Player.UserAccount.Id;

                        using (var session = EssentialsDb.CreateSession())
                        {
                            using (var transaction = session.BeginTransaction())
                            {
                                Ban newBan = new Ban
                                {
                                    IPAddress = plr.PlayerClient.RemoteEndPoint.Address.ToString(),
                                    UserAccountId = acctId,
                                    BanReason = banReason,
                                    ExpirationTime = expireTime
                                };

                                session.Save(newBan);

                                session.Save(new BanUUID
                                {
                                    Ban = newBan,
                                    PlayerName = plr.Player.Name,
                                    UUID = plr.Player.UUID
                                });

                                transaction.Commit();
                            }
                        }

                        plr.ServerClient.ClientDisconnected += (sender, e) =>
                        {
                            if (kickBanner.PlayerClient.Connected)
                                kickBanner.PlayerClient.SendChatMessage("Server", String.Format("Player {0} has been banned!", plr.Player.Name));

                            Logger.Info("Player {0} has been banned by {1} ({2})", e.Client.Server.Player.Name, kickBanner.Player.Name, kickBanner.Player.UserAccount.Username);

                        };
                    }

                    Thread.Sleep(1000);

                    if (plr.PlayerClient != null)
                        plr.PlayerClient.ForceDisconnect();

                    if (plr.ServerClient != null)
                        plr.ServerClient.ForceDisconnect();

                });

            }
        }

        public static bool IsAdmin(SharpStarClient client)
        {
            return client.IsAdmin();
        }

        public static bool CanUserAccess(SharpStarClient client, string command, bool sendMsg = true)
        {
            return client.CanUserAccess(command, sendMsg);
        }

    }
}

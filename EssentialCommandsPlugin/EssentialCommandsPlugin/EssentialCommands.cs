using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EssentialCommandsPlugin.Commands;
using EssentialCommandsPlugin.ConsoleCommands;
using Mono.Addins;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Database;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

[assembly: Addin("EssentialCommands", Version = "1.0.5.1")]
[assembly: AddinDescription("A command plugin that is essential")]
[assembly: AddinProperty("sharpstar", "0.2.3.2")]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]

namespace EssentialCommandsPlugin
{
    [Extension]
    public class EssentialCommands : CSPlugin
    {

        public static EssentialCommandsConfig Config;

        private const string DatabaseName = "essentialcommands.db";

        private const string ConfigFileName = "essentialcommands.json";

        public static readonly EssentialCommandsDb Database = new EssentialCommandsDb(DatabaseName);

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


        public override void OnLoad()
        {

            Config = new EssentialCommandsConfig(ConfigFileName);
            Config.Save();

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

        public override bool OnChatCommandReceived(SharpStarClient client, string command, string[] args)
        {

            if (client.Server.Player.UserGroupId.HasValue && client.Server.Player.UserAccount != null)
            {

                var cmd = Database.GetCommand(client.Server.Player.UserGroupId.Value, command);

                if (cmd != null)
                {

                    Database.IncCommandTimesUsed(client.Server.Player.UserAccount.Id, cmd.GroupId, command);

                    var usrCmd = Database.GetUserCommand(client.Server.Player.UserAccount.Id, cmd.GroupId, command);

                    if (usrCmd != null)
                    {
                        if (cmd.Limit < usrCmd.TimesUsed)
                        {

                            client.SendChatMessage("Server", "You have reached the limit for this command!");

                            return true;

                        }
                    }

                }

            }

            return base.OnChatCommandReceived(client, command, args);

        }

        public static void KickBanPlayer(SharpStarServerClient kickBanner, List<SharpStarServerClient> players, bool ban = false)
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

                Task.Factory.StartNew(() =>
                {

                    if (!ban)
                    {
                        plr.ServerClient.ClientDisconnected += (sender, e) =>
                        {
                            if (e.Client.Connected)
                                kickBanner.PlayerClient.SendChatMessage("Server", String.Format("Player {0} has been kicked!", plr.Player.Name));

                            Logger.Info("Player {0} has kicked by {1} ({2})", e.Client.Server.Player.Name, kickBanner.Player.Name, kickBanner.Player.UserAccount.Username);
                        };
                    }
                    else
                    {

                        int? acctId = null;

                        if (plr.Player.UserAccount != null)
                            acctId = plr.Player.UserAccount.Id;

                        Database.AddBan(plr.Player.UUID, acctId);

                        plr.ServerClient.ClientDisconnected += (sender, e) =>
                        {
                            if (e.Client.Connected)
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

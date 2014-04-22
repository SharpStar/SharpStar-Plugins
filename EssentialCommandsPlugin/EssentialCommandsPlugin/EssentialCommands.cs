using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Addins;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

[assembly: Addin]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]

namespace EssentialCommandsPlugin
{
    [Extension]
    public class EssentialCommands : CSPlugin
    {

        private EssentialCommandsConfig _config;

        private const string DatabaseName = "essentialcommands.db";

        private const string ConfigFileName = "essentialcommands.json";


        private readonly EssentialCommandsDb _database;

        public override string Name
        {
            get { return "Essential Commands"; }
        }

        public EssentialCommands()
        {
            _database = new EssentialCommandsDb(DatabaseName);
        }

        public override void OnLoad()
        {

            RegisterCommand("makeadmin", MakeAdmin);
            RegisterCommand("removeadmin", RemoveAdmin);
            RegisterCommand("kick", KickPlayer);
            RegisterCommand("ban", BanPlayer);
            RegisterCommand("broadcast", Broadcast);
            RegisterCommand("giveitem", GiveItem);
            RegisterCommand("addperm", AddPermission);
            RegisterCommand("removeperm", RemovePermission);
            RegisterCommand("setmotd", SetMotd);
            RegisterCommand("warpto", WarpToPlayer);
            RegisterCommand("warptome", WarpToMe);
            RegisterCommand("kill", KillPlayer);

            RegisterEvent("connectionResponse", ConnectionResponse);
            RegisterEvent("afterConnectionResponse", AfterConnectionResponse);

            _config = new EssentialCommandsConfig(ConfigFileName);

        }

        private void ConnectionResponse(IPacket packet, StarboundClient client)
        {

            ConnectionResponsePacket crp = (ConnectionResponsePacket)packet;

            var bans = _database.GetBans();

            if (bans.Any(p => p.UUID == client.Server.Player.UUID))
            {

                crp.Success = false;
                crp.RejectionReason = "You are banned!";

                Console.WriteLine("Banned player {0} ({1}) tried to join", client.Server.Player.Name, client.Server.Player.UUID);

            }
            else if (client.Server.Player.UserAccount != null && bans.Any(p => p.UserAccountId.HasValue && p.UserAccountId.Value == client.Server.Player.UserAccount.Id))
            {

                crp.Success = false;
                crp.RejectionReason = "You are banned!";

                Console.WriteLine("Banned user {0} ({1}) tried to join", client.Server.Player.UserAccount.Username, client.Server.Player.UUID);

            }

        }

        private void AfterConnectionResponse(IPacket packet, StarboundClient client)
        {

            if (!string.IsNullOrEmpty(_config.ConfigFile.Motd)) //if motd has been set
            {
                client.SendChatMessage("MOTD", _config.ConfigFile.Motd);
            }

        }

        private void MakeAdmin(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string username = string.Join(" ", args);

            var user = SharpStarMain.Instance.Database.GetUser(username);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            if (user.IsAdmin)
            {

                client.SendChatMessage("Server", "The user is already an admin!");

                return;

            }

            SharpStarMain.Instance.Database.ChangeAdminStatus(user.Id, true);

        }

        private void RemoveAdmin(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string username = string.Join(" ", args);

            var user = SharpStarMain.Instance.Database.GetUser(username);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            if (!user.IsAdmin)
            {

                client.SendChatMessage("Server", "The user is not an admin!");

                return;

            }

            SharpStarMain.Instance.Database.ChangeAdminStatus(user.Id, false);

        }

        private void KickPlayer(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("kick"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string playerName = string.Join(" ", args);

            var players = SharpStarMain.Instance.Server.Clients.Where(p => p.Player.Name.Equals(playerName)).ToList();

            if (players.Count == 0)
            {

                client.SendChatMessage("Server", "There are no players by that name!");

                return;

            }

            KickBanPlayer(client.Server, players);

        }

        private void BanPlayer(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("ban"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string playerName = string.Join(" ", args);

            var players = SharpStarMain.Instance.Server.Clients.Where(p => p.Player.Name.Equals(playerName)).ToList();

            if (players.Count == 0)
            {

                client.SendChatMessage("Server", "There are no players by that name!");

                return;

            }

            KickBanPlayer(client.Server, players, true);

        }

        private void Broadcast(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("broadcast"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string message = string.Join(" ", args);

            foreach (var cl in SharpStarMain.Instance.Server.Clients)
            {
                cl.PlayerClient.SendChatMessage("Broadcast", message);
            }

        }

        private void GiveItem(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("giveitem"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 3)
            {

                client.SendChatMessage("Server", "Syntax: /giveitem <material name> <amount> <player name>");

                return;

            }

            string item = args[0];
            string countStr = args[1];
            string player = string.Join("", string.Join(" ", args.Skip(2))); //skip item, count

            var plr = SharpStarMain.Instance.Server.Clients.Where(p => p.Player.Name.Equals(player, StringComparison.OrdinalIgnoreCase)).ToList();

            if (plr.Count == 0)
            {

                client.SendChatMessage("Server", String.Format("Player {0} is not online!", player));

                return;

            }

            int count;

            if (!int.TryParse(countStr, out count))
            {

                client.SendChatMessage("Server", "Syntax: /giveitem <material name> <amount> <player name>");

                return;

            }

            var packet = new GiveItemPacket();
            packet.ItemName = item;
            packet.Count = (ulong)count + 1;

            plr[0].PlayerClient.SendPacket(packet);

        }

        private void AddPermission(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /addperm <permission> <username>");

                return;

            }

            string perm = args[0];
            string userName = args[1];

            var user = SharpStarMain.Instance.Database.GetUser(userName);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            SharpStarMain.Instance.Database.AddPlayerPermission(user.Id, perm, true);

        }

        private void RemovePermission(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /removeperm <permission> <username>");

                return;

            }

            string perm = args[0];
            string userName = args[1];

            var user = SharpStarMain.Instance.Database.GetUser(userName);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            SharpStarMain.Instance.Database.DeletePlayerPermission(user.Id, perm);

        }

        private void SetMotd(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("setmotd"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string motd = string.Join(" ", args);

            _config.ConfigFile.Motd = motd;
            _config.Save();

            client.SendChatMessage("Server", "MOTD Set!");

        }

        private void WarpToPlayer(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("warp"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /warpto <player name>");

                return;

            }

            string playerName = string.Join(" ", args);

            var playerClient = SharpStarMain.Instance.Server.Clients.FirstOrDefault(p => p.Player.Name.Equals(playerName));

            if (playerClient == null)
            {

                client.SendChatMessage("Server", String.Format("The player {0} is not online!", playerName));

                return;

            }

            client.WarpTo(playerClient.Player);

        }

        private void WarpToMe(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("warp"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /warptome <player name>");

                return;

            }

            string playerName = string.Join(" ", args);

            var playerClient = SharpStarMain.Instance.Server.Clients.FirstOrDefault(p => p.Player.Name.Equals(playerName));

            if (playerClient == null)
            {

                client.SendChatMessage("Server", String.Format("The player {0} is not online!", playerName));

                return;

            }

            playerClient.PlayerClient.WarpTo(client.Server.Player);

        }

        private void KillPlayer(StarboundClient client, string[] args)
        {

            if (!IsAdmin(client) && !client.Server.Player.HasPermission("kill"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /kill <player name>");

                return;

            }

            string playerName = string.Join(" ", args);

            var playerClient = SharpStarMain.Instance.Server.Clients.FirstOrDefault(p => p.Player.Name.Equals(playerName));

            if (playerClient == null)
            {

                client.SendChatMessage("Server", String.Format("The player {0} is not online!", playerName));

                return;

            }

            EntityDestroyPacket dest = new EntityDestroyPacket();
            dest.EntityId = playerClient.Player.EntityId;
            dest.Death = true;
            dest.Unknown = new byte[0];

            foreach (var cl in SharpStarMain.Instance.Server.Clients)
            {
                cl.PlayerClient.SendPacket(dest);
            }

        }

        private void KickBanPlayer(StarboundServerClient kickBanner, List<StarboundServerClient> players, bool ban = false)
        {

            for (int i = 0; i < players.Count; i++)
            {

                var plr = players[i];

                if (!plr.Connected)
                    continue;

                if (!ban)
                    plr.PlayerClient.SendChatMessage("Server", "You are being kicked. Goodbye.");
                else
                    plr.PlayerClient.SendChatMessage("Server", "You have been banned. Goodbye.");

                Task.Factory.StartNew(() =>
                {

                    Thread.Sleep(1500);

                    if (!ban)
                    {
                        plr.Disconnected += (sender, e) => kickBanner.PlayerClient.SendChatMessage("Server", String.Format("Player {0} has been kicked!", plr.Player.Name));
                    }
                    else
                    {

                        int? acctId = null;

                        if (plr.Player.UserAccount != null)
                            acctId = plr.Player.UserAccount.Id;

                        _database.AddBan(plr.Player.UUID, acctId);

                        plr.Disconnected += (sender, e) => kickBanner.PlayerClient.SendChatMessage("Server", String.Format("Player {0} has been banned!", plr.Player.Name));

                    }

                    plr.ForceDisconnect();

                });

            }
        }

        private bool IsAdmin(StarboundClient client)
        {
            return client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.IsAdmin;
        }

    }
}

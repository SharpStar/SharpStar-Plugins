using System;
using System.Linq;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class BanCommand
    {

        [Command("ban")]
        public void BanPlayer(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("ban"))
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

            EssentialCommands.KickBanPlayer(client.Server, players, true);

        }

        [Command("unban")]
        public void UnbanPlayer(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("ban"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /unban <player name>");

                return;

            }

            string username = args[0];

            var usr = SharpStarMain.Instance.Database.GetUser(username);

            if (usr == null)
            {

                client.SendChatMessage("Server", "There is no user by that name!");

                return;

            }

            EssentialCommands.Database.RemoveBan(usr.Id);

            client.SendChatMessage("Server", "The user is no longer banned!");

        }

        [Event("connectionResponse")]
        public void ConnectionResponse(IPacket packet, StarboundClient client)
        {

            ConnectionResponsePacket crp = (ConnectionResponsePacket)packet;

            var bans = EssentialCommands.Database.GetBans();

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

    }
}

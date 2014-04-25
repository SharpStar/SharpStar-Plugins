using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class WarpCommands
    {


        [Command("warpto")]
        public void WarpToPlayer(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("warp"))
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

            var playerClient = SharpStarMain.Instance.Server.Clients.FirstOrDefault(p => p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

            if (playerClient == null)
            {

                client.SendChatMessage("Server", String.Format("The player {0} is not online!", playerName));

                return;

            }

            client.WarpTo(playerClient.Player);

        }

        [Command("warptome")]
        public void WarpToMe(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("warp"))
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

            var playerClient = SharpStarMain.Instance.Server.Clients.FirstOrDefault(p => p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

            if (playerClient == null)
            {

                client.SendChatMessage("Server", String.Format("The player {0} is not online!", playerName));

                return;

            }

            playerClient.PlayerClient.WarpTo(client.Server.Player);

        }

    }
}

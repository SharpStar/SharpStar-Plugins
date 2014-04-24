using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class KickCommand
    {

        [Command("kick")]
        public void KickPlayer(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("kick"))
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

            EssentialCommands.KickBanPlayer(client.Server, players);

        }

    }
}

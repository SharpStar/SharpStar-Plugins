using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class KickCommand
    {

        [Command("kick", "Kick a player")]
        [CommandPermission("kick")]
        public void KickPlayer(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "kick"))
                return;

            string playerName = string.Join(" ", args);

            var players = SharpStarMain.Instance.Server.Clients.Where(p => p.Player != null && p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (players.Count == 0)
            {
                client.SendChatMessage("Server", "There are no players by that name!");

                return;
            }

            EssentialCommands.KickBanPlayer(client.Server, players);

        }

    }
}

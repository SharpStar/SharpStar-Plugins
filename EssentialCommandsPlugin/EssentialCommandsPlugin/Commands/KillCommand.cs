using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssentialCommandsPlugin.Attributes;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class KillCommand
    {

        [Command("kill", "Kill a player")]
        [CommandPermission("kill")]
        public void KillPlayer(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "kill"))
                return;

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

    }
}

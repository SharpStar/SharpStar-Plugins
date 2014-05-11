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
    public class GiveItemCommand
    {

        [Command("giveitem", "Give a player an item")]
        [CommandPermission("giveitem")]
        public void GiveItem(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "giveitem"))
                return;
            
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

    }
}

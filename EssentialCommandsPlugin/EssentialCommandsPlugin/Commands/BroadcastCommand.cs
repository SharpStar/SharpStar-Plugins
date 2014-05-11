using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssentialCommandsPlugin.Attributes;
using SharpStar.Lib;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class BroadcastCommand
    {

        [Command("broadcast", "Broadcast a message to the server")]
        [CommandPermission("broadcast")]
        public void Broadcast(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "broadcast"))
                return;

            string message = string.Join(" ", args);

            foreach (var cl in SharpStarMain.Instance.Server.Clients)
            {
                cl.PlayerClient.SendChatMessage("Broadcast", message);
            }

        }

    }
}

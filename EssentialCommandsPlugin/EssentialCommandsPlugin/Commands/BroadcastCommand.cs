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
    public class BroadcastCommand
    {

        [Command("broadcast", "Broadcast a message to the server")]
        [CommandPermission("broadcast")]
        public void Broadcast(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "broadcast"))
                return;

            string message = string.Join(" ", args);

            foreach (var cl in SharpStarMain.Instance.Server.Clients.ToList())
            {
                cl.PlayerClient.SendChatMessage("Broadcast", message);
            }

        }

    }
}

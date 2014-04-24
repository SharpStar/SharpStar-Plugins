using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class BroadcastCommand
    {

        [Command("broadcast")]
        public void Broadcast(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("broadcast"))
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

    }
}

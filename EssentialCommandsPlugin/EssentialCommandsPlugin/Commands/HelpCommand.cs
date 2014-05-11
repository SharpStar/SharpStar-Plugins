using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class HelpCommand
    {

        [Command("help", "Show a list of commands")]
        public void ShowHelp(StarboundClient client, string[] args)
        {

            client.SendChatMessage("Server", "-- List of Commands --");

            foreach (var cmd in EssentialCommands.Commands)
            {
                if (EssentialCommands.IsAdmin(client) || EssentialCommands.CanUserAccess(client, cmd.Item1, false))
                    client.SendChatMessage("Server", String.Format("/{0} - {1}", cmd.Item1, cmd.Item2));
            }

        }

    }
}

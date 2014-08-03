using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace ServerManagementPlugin.Commands
{
    public class ServerCommand
    {
        [CommandPermission("server")]
        [Command("server", "Server Management")]
        public void Server(StarboundClient client, string[] args)
        {

            if (!client.CanUserAccess("server", true))
                return;

            if (args.Length == 0)
            {
                client.SendChatMessage("Server", "Syntax: server <command> <arguments...>");
                client.SendChatMessage("Server", "Commands: restart, stop, set");

                return;
            }

            switch (args[0])
            {

                case "stop":

                    ServerManagement.StopServer(ServerManagement.GetStarboundProcess());

                    break;

                case "restart":

                    int restartIn = 0;

                    if (args.Length == 2)
                    {
                        int.TryParse(args[1], out restartIn);
                    }

                    if (restartIn > 0)
                    {
                        Task.Factory.StartNew(() =>
                        {

                            if (!string.IsNullOrEmpty(ServerManagement.Config.ConfigFile.ServerRestartMessage))
                            {
                                foreach (StarboundServerClient cl in SharpStarMain.Instance.Server.Clients)
                                {
                                    cl.PlayerClient.SendChatMessage("Server", ServerManagement.Config.ConfigFile.ServerRestartMessage);
                                }
                            }

                            for (int i = 0; i < restartIn; i++)
                            {
                                Thread.Sleep(1000); //1 second

                                foreach (StarboundServerClient cl in SharpStarMain.Instance.Server.Clients)
                                {
                                    cl.PlayerClient.SendChatMessage("Server", (restartIn - i) + "...");
                                }
                            }

                        }).ContinueWith(t => ServerManagement.RestartServer());
                    }
                    else
                    {
                        ServerManagement.RestartServer();
                    }

                    break;

                case "set":

                    if (args.Length < 3)
                    {
                        client.SendChatMessage("Server", "Syntax: server set <property> <value>");
                        client.SendChatMessage("Server", "Properties: autorestart, intervalrestart");

                        return;
                    }

                    switch (args[1].ToLower())
                    {

                        case "autorestart":

                            bool on = args[2].Equals("on", StringComparison.OrdinalIgnoreCase);

                            if (on || args[2].Equals("off", StringComparison.OrdinalIgnoreCase))
                            {
                                ServerManagement.Config.ConfigFile.AutoRestartOnCrash = on;
                            }
                            else
                            {
                                client.SendChatMessage("Server", "Syntax: server set autorestart <on/off>");
                            }

                            break;

                        case "intervalrestart":

                            int interval;

                            if (!int.TryParse(args[2], out interval))
                                client.SendChatMessage("Server", "Invalid number!");
                            else
                                ServerManagement.Config.ConfigFile.ServerCheckInterval = interval;

                            break;

                        default:
                            client.SendChatMessage("Server", "Syntax: server set <property> <value>");
                            client.SendChatMessage("Server", "Properties: autorestart, intervalrestart");
                            break;

                    }

                    ServerManagement.Config.Save();

                    break;
            }

        }

    }
}

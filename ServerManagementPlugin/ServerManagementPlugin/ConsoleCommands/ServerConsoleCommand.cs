using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpStar.Lib;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Mono;
using SharpStar.Lib.Plugins;
using System.Management;
using SharpStar.Lib.Server;

namespace ServerManagementPlugin.ConsoleCommands
{
    public class ServerConsoleCommand
    {

        private static readonly SharpStarLogger Logger = ServerManagement.Logger;

        [ConsoleCommand("server", "Server Management")]
        public void Server(string[] args)
        {

            if (args.Length == 0)
            {
                Logger.Info("Syntax: server <command> <arguments...>");
                Logger.Info("Commands: start, restart, stop, set");

                return;
            }

            switch (args[0])
            {

                case "start":

                    ServerManagement.StartServer();

                    break;

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

                            Logger.Info("Restarting in:");

                            for (int i = 0; i < restartIn; i++)
                            {
                                Thread.Sleep(1000); //1 second

                                Logger.Info((restartIn - i) + "...");

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
                        Logger.Info("Syntax: server set <property> <value>");
                        Logger.Info("Properties: autorestart, intervalrestart");

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
                                Logger.Info("Syntax: server set autorestart <on/off>");
                            }

                            break;

                        case "intervalrestart":

                            int interval;

                            if (!int.TryParse(args[2], out interval))
                                Logger.Error("Invalid number!");
                            else
                                ServerManagement.Config.ConfigFile.ServerCheckInterval = interval;

                            break;

                        default:
                            Logger.Info("Syntax: server set <property> <value>");
                            Logger.Info("Properties: autorestart, intervalrestart");
                            break;

                    }

                    ServerManagement.Config.Save();

                    break;
            }

        }

    }
}

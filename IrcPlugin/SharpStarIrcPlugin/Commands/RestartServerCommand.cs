﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Meebey.SmartIrc4net;
using SharpStar.Lib;

namespace SharpStarIrcPlugin.Commands
{
    public class RestartServerCommand : IrcCommand
    {
        public override string CommandName
        {
            get { return "restart"; }
        }

        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            var auth = plugin.AuthenticatedUsers.SingleOrDefault(p => p.Key.Equals(nick, StringComparison.OrdinalIgnoreCase));

            if (auth.Value != null)
            {

                var perm = SharpStarMain.Instance.Database.GetPlayerPermission(auth.Value.Id, "ircserver");

                if (perm != null || auth.Value.IsAdmin)
                {

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    Assembly assm = assemblies.SingleOrDefault(p => p.GetTypes().Any(x => x.FullName == "ServerManagementPlugin.ServerManagement"));

                    if (assm == null)
                    {
                        plugin.Irc.SendMessage(SendType.Message, channel, "Server Management Plugin is required to use this command!");

                        return;
                    }

                    if (args.Length == 0)
                    {
                        SharpStarMain.Instance.PluginManager.PassConsoleCommand("server", new[] { "restart" });
                    }
                    else
                    {
                        SharpStarMain.Instance.PluginManager.PassConsoleCommand("server", new[] { "restart" }.Union(args).ToArray());
                    }

                    plugin.Irc.SendMessage(SendType.Message, channel, "Restarting Starbound server....");

                }
                else
                {
                    plugin.Irc.SendMessage(SendType.Message, channel, "You do not have permission to use this command!");
                }
            }
            else
            {
                plugin.Irc.SendMessage(SendType.Message, channel, "You must be logged in to use this command!");
            }

        }
    }
}

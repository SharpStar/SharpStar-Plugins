using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace SharpStarIrcPlugin.QueryCommands
{
    public class LogoutCommand : IrcCommand
    {
        public override string CommandName
        {
            get { return "logout"; }
        }

        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            var auth = plugin.AuthenticatedUsers.SingleOrDefault(p => p.Key.Equals(nick, StringComparison.OrdinalIgnoreCase));

            if (auth.Value != null)
            {

                plugin.AuthenticatedUsers.Remove(nick);

                plugin.Irc.SendMessage(SendType.Message, nick, "You have logged out successfully!");

            }

        }
    }
}

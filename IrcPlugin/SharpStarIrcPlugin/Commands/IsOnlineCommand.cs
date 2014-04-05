using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using SharpStar.Lib;

namespace SharpStarIrcPlugin.Commands
{
    public class IsOnlineCommand : IrcCommand
    {
        public override string CommandName
        {
            get { return "isonline"; }
        }

        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            if (args.Length == 0)
            {

                plugin.Irc.SendMessage(SendType.Message, channel, String.Format("Syntax: {0} <player name>", IrcPlugin.Config.Config.CommandPrefix));

                return;

            }

            string plrName = string.Join(" ", args);

            if (SharpStarMain.Instance.Server.Clients.Any(p => p.Player != null && p.Player.Name.Equals(plrName, StringComparison.OrdinalIgnoreCase)))
            {
                plugin.Irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0} is online!", IrcConstants.IrcBold, plrName));
            }
            else
            {
                plugin.Irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0} is offline!", IrcConstants.IrcBold, plrName));
            }

        }
    }
}

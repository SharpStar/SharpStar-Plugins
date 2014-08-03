using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using SharpStar.Lib;
using SharpStar.Lib.Server;

namespace SharpStarIrcPlugin.Commands
{
    public class SayCommand : IrcCommand
    {
        public override string CommandName
        {
            get
            {
                return "say";
            }
        }
        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            string message = string.Join(" ", args);

            foreach (StarboundServerClient ssc in SharpStarMain.Instance.Server.Clients)
            {
                try
                {
                    ssc.PlayerClient.SendChatMessage(nick, message);
                }
                catch (Exception)
                {
                }
            }

            plugin.Irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0}: {2}", IrcConstants.IrcBold, nick, message));

        }
    }
}

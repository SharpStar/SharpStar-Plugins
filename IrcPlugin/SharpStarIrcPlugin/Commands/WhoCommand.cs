using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using SharpStar.Lib;
using SharpStar.Lib.Server;

namespace SharpStarIrcPlugin.Commands
{
    public class WhoCommand : IrcCommand
    {
        public override string CommandName
        {
            get { return "who"; }
        }

        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            List<StarboundServerClient> clients;

            int page = 1;

            if (args.Length > 0)
            {

                int.TryParse(args[0], out page);

                int pg = page - 1;

                clients = SharpStarMain.Instance.Server.Clients.Skip(pg * 5).Take(5).ToList();

            }
            else
            {
                clients = SharpStarMain.Instance.Server.Clients.Take(5).ToList();
            }

            plugin.Irc.SendMessage(SendType.Message, channel, String.Format("{0}Players:{0}", IrcConstants.IrcBold));

            foreach (var cl in clients)
            {
                if (cl.Player != null && cl.Player.UserAccount != null)
                    plugin.Irc.SendMessage(SendType.Message, channel, String.Format("{0} ({1})", cl.Player.Name, cl.Player.UserAccount.Username));
                else if (cl.Player != null)
                    plugin.Irc.SendMessage(SendType.Message, channel, String.Format("{0}", cl.Player.Name));
            }

            plugin.Irc.SendMessage(SendType.Message, channel, String.Format("Page {0}{1}{0} of {0}{2}{0}", IrcConstants.IrcBold, page, (int)Math.Ceiling(((double)SharpStarMain.Instance.Server.Clients.Count / 5))));

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using SharpStar.Lib;

namespace SharpStarIrcPlugin.Commands
{
    public class KickCommand : IrcCommand
    {
        public override string CommandName
        {
            get { return "kick"; }
        }

        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            var auth = plugin.AuthenticatedUsers.SingleOrDefault(p => p.Key.Equals(nick, StringComparison.OrdinalIgnoreCase));

            if (auth.Value != null)
            {

                var perm = SharpStarMain.Instance.Database.GetPlayerPermission(auth.Value.Id, "irckick");

                if (perm != null || auth.Value.IsAdmin)
                {

                    string joined = string.Join(" ", args);

                    var player = SharpStarMain.Instance.Server.Clients.SingleOrDefault(p => p.Player.Name.Equals(joined, StringComparison.OrdinalIgnoreCase));

                    if (player != null)
                    {

                        if (player.ServerClient.CheckConnection())
                            player.ForceDisconnect();

                        plugin.Irc.SendMessage(SendType.Message, channel, String.Format("Player {0}{1}{0} has been kicked!", IrcConstants.IrcBold, joined));

                    }
                    else
                    {
                        plugin.Irc.SendMessage(SendType.Message, channel, "This player is not online!");
                    }

                }
                else
                {
                    plugin.Irc.SendMessage(SendType.Message, channel, "You do not have permission to do this!");
                }

            }
            else
            {
                plugin.Irc.SendMessage(SendType.Message, channel, "You must be logged in to do this!");
            }

        }
    }
}

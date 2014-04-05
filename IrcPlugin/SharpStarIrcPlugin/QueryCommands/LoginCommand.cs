using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using SharpStar.Lib;
using SharpStar.Lib.Database;
using SharpStar.Lib.Security;

namespace SharpStarIrcPlugin.QueryCommands
{
    public class LoginCommand : IrcCommand
    {
        public override string CommandName
        {
            get { return "login"; }
        }

        public override void ParseCommand(IrcPlugin plugin, string channel, string nick, string[] args)
        {

            if (args.Length != 2)
            {

                plugin.Irc.SendMessage(SendType.Message, nick, "Syntax: login <username> <password>");

                return;

            }

            SharpStarUser user = SharpStarMain.Instance.Database.GetUser(args[0]);

            if (user == null)
            {

                plugin.Irc.SendMessage(SendType.Message, nick, "Incorrect username/password");

                return;

            }

            var auth = plugin.AuthenticatedUsers.SingleOrDefault(p => p.Key.Equals(nick, StringComparison.OrdinalIgnoreCase));

            if (auth.Value != null)
            {

                plugin.Irc.SendMessage(SendType.Message, nick, "You have already authenticated!");

                return;

            }

            var userAuth = plugin.AuthenticatedUsers.SingleOrDefault(p => p.Value.Id == user.Id);

            if (userAuth.Value != null)
            {

                plugin.Irc.SendMessage(SendType.Message, nick, "Someone is already logged into this account!");

                return;

            }

            string hash = SharpStarSecurity.GenerateHash(user.Username, args[1], user.Salt, 5000);

            if (user.Hash != hash)
            {

                plugin.Irc.SendMessage(SendType.Message, nick, "Incorrect username/password");

                return;

            }

            plugin.Irc.SendMessage(SendType.Message, nick, "Login Successful!");
            plugin.Irc.SendMessage(SendType.Message, nick, "NOTE: Remember to logout when disconnecting from irc or changing your nick!");


            plugin.AuthenticatedUsers.Add(nick, user);

        }
    }
}

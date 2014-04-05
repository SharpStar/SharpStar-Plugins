using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace SharpStarIrcPlugin
{
    public abstract class IrcCommand
    {

        public abstract string CommandName { get; }

        public abstract void ParseCommand(IrcPlugin instance , string channel, string nick, string[] args);

    }
}

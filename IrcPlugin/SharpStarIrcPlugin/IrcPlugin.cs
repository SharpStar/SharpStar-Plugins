using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Meebey.SmartIrc4net;
using Mono.Addins;
using SharpStar.Lib;
using SharpStar.Lib.Database;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;
using SharpStarIrcPlugin.Commands;
using SharpStarIrcPlugin.QueryCommands;

[assembly: Addin]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]

namespace SharpStarIrcPlugin
{
    [Extension]
    public class IrcPlugin : CSPlugin
    {

        private const string DefaultConfigFile = "ircplugin.json";

        private volatile bool _running;

        public static IrcConfig Config = new IrcConfig(DefaultConfigFile);

        public IrcClient Irc;

        private Thread _ircThread;

        private static readonly List<IrcCommand> Commands;

        private static readonly List<IrcCommand> QueryCommands;

        public Dictionary<string, SharpStarUser> AuthenticatedUsers { get; private set; }

        public override string Name
        {
            get { return "IRC Plugin"; }
        }

        static IrcPlugin()
        {

            Commands = new List<IrcCommand>();
            Commands.Add(new SayCommand());
            Commands.Add(new WhoCommand());
            Commands.Add(new IsOnlineCommand());
            Commands.Add(new KickCommand());
            Commands.Add(new StartServerCommand());
            Commands.Add(new StopServerCommand());
            Commands.Add(new RestartServerCommand());

            QueryCommands = new List<IrcCommand>();
            QueryCommands.Add(new LoginCommand());
            QueryCommands.Add(new LogoutCommand());

        }

        public IrcPlugin()
        {

            _running = false;

            RegisterEvent("chatSent", ChatSent);
            RegisterEvent("clientConnected", ClientConnected);
            RegisterEvent("clientDisconnected", ClientDisconnected);

            AuthenticatedUsers = new Dictionary<string, SharpStarUser>();

        }

        void _irc_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("[Irc Plugin] ERROR: {0}", e.ErrorMessage);
        }

        public override void OnLoad()
        {

            Config.Reload();

            if (Config.Config.Channels.Count > 0)
            {

                if (Irc != null && Irc.IsConnected)
                    Irc.Disconnect();

                _ircThread = new Thread(RunIrc);

                _ircThread.Start();

            }

        }

        public override void OnUnload()
        {

            _running = false;

            if (_ircThread != null)
                _ircThread.Abort(); //TODO: figure out a way to avoid this

        }

        private void RunIrc()
        {

            try
            {

                _running = true;

                Thread.CurrentThread.Name = "IrcThread";

                Irc = new IrcClient();
                Irc.OnError += _irc_OnError;
                Irc.OnChannelMessage += _irc_OnChannelMessage;
                Irc.OnQueryMessage += _irc_OnQueryMessage;
                Irc.ActiveChannelSyncing = true;

                Irc.Connect(Config.Config.IrcNetwork, Config.Config.IrcPort);

                if (string.IsNullOrEmpty(Config.Config.Password))
                    Irc.Login(Config.Config.Nick, "SharpStar Bot");
                else
                    Irc.Login(Config.Config.Nick, "SharpStar Bot", 0, Config.Config.Nick, Config.Config.Password);

                foreach (IrcChannel chan in Config.Config.Channels)
                {
                    if (chan.Password != null)
                        Irc.RfcJoin(chan.Channel, chan.Password);
                    else
                        Irc.RfcJoin(chan.Channel);
                }

                while (_running)
                {

                    if (!Irc.IsConnected)
                        break;

                    Irc.ListenOnce();

                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Irc Plugin] ERROR: {0}", ex.Message);
            }
            finally
            {

                if (Irc.IsConnected)
                {
                    Irc.RfcQuit("Goodbye.");
                    Irc.Disconnect();
                }
            
            }

        }

        private void _irc_OnQueryMessage(object sender, IrcEventArgs e)
        {

            if (e.Data.Type == ReceiveType.QueryMessage)
            {

                IrcCommand cmd = QueryCommands.SingleOrDefault(p => p.CommandName.Equals(e.Data.MessageArray[0], StringComparison.OrdinalIgnoreCase));

                if (cmd != null)
                    cmd.ParseCommand(this, null, e.Data.Nick, e.Data.MessageArray.Skip(1).ToArray());

            }

        }

        private void _irc_OnChannelMessage(object sender, IrcEventArgs e)
        {

            string channel = e.Data.Channel;

            string[] ex = Regex.Split(e.Data.Message, Config.Config.CommandPrefix);

            if (ex.Length > 1)
            {

                string joined = string.Join("", ex.Skip(1));

                string[] ex2 = joined.Split(' ');

                if (ex2.Length > 0)
                {

                    IrcCommand cmd = Commands.SingleOrDefault(p => p.CommandName.Equals(ex2[0], StringComparison.OrdinalIgnoreCase));

                    if (cmd != null)
                        cmd.ParseCommand(this, channel, e.Data.Nick, ex2.Skip(1).ToArray());

                }

            }

        }

        private void ChatSent(IPacket packet, StarboundClient client)
        {

            var csp = packet as ChatSentPacket;

            if (csp != null && !csp.Message.StartsWith("/") && Irc != null && Irc.IsConnected)
            {

                foreach (string channel in Config.Config.Channels.Select(p => p.Channel))
                {
                    Irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0}: {2}", IrcConstants.IrcBold, client.Server.Player.Name, csp.Message));
                }

            }
        }

        private void ClientConnected(IPacket packet, StarboundClient client)
        {

            var ccp = packet as ClientConnectPacket;

            if (ccp != null && Irc != null && Irc.IsConnected)
            {

                foreach (string channel in Config.Config.Channels.Select(p => p.Channel))
                {
                    Irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0} connected", IrcConstants.IrcBold, client.Server.Player.Name));
                }

            }

        }

        private void ClientDisconnected(IPacket packet, StarboundClient client)
        {

            var cdp = packet as ClientDisconnectPacket;

            if (cdp != null && Irc != null && Irc.IsConnected)
            {

                foreach (string channel in Config.Config.Channels.Select(p => p.Channel))
                {
                    Irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0} disconnected", IrcConstants.IrcBold, client.Server.Player.Name));
                }

            }

        }

    }
}

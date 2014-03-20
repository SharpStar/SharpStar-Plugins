using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Meebey.SmartIrc4net;
using Mono.Addins;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

[assembly: Addin]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]

namespace SharpStarIrcPlugin
{
    [Extension]
    public class IrcPlugin : CSPlugin
    {

        private const string DefaultConfigFile = "ircplugin.json";

        private volatile bool _running;

        private static readonly IrcConfig Config = new IrcConfig(DefaultConfigFile);

        private IrcClient _irc;

        private Thread _ircThread;

        public override string Name
        {
            get { return "IRC Plugin"; }
        }

        public IrcPlugin()
        {

            _running = false;

            RegisterEvent("chatSent", ChatSent);
            RegisterEvent("clientConnected", ClientConnected);
            RegisterEvent("clientDisconnected", ClientDisconnected);

        }

        void _irc_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("[Irc Plugin] ERROR: {0}", e.ErrorMessage);
        }

        public override void OnLoad()
        {

            _ircThread = new Thread(RunIrc);

            _ircThread.Start();

        }

        public override void OnUnload()
        {

            _running = false;

            _ircThread.Abort(); //TODO: figure out a way to avoid this

        }

        private void RunIrc()
        {

            try
            {

                _running = true;

                Thread.CurrentThread.Name = "IrcThread";

                _irc = new IrcClient();
                _irc.OnError += _irc_OnError;
                _irc.ActiveChannelSyncing = true;

                _irc.Connect(Config.Config.IrcNetwork, Config.Config.IrcPort);

                _irc.Login(Config.Config.Nick, "SharpStar Bot");

                _irc.RfcJoin(Config.Config.Channels.ToArray());

                while (_running)
                {
                    _irc.ListenOnce();
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
                _irc.RfcQuit("Goodbye.");
                _irc.Disconnect();
            }

        }

        private void ChatSent(IPacket packet, StarboundClient client)
        {

            var csp = packet as ChatSentPacket;

            if (csp != null && !csp.Message.StartsWith("/"))
            {

                foreach (string channel in Config.Config.Channels)
                {
                    _irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0}: {2}", IrcConstants.IrcBold, client.Server.Player.Name, csp.Message));
                }

            }
        }

        private void ClientConnected(IPacket packet, StarboundClient client)
        {

            var ccp = packet as ClientConnectPacket;

            if (ccp != null)
            {

                foreach (string channel in Config.Config.Channels)
                {
                    _irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0} connected", IrcConstants.IrcBold, client.Server.Player.Name));
                }

            }

        }

        private void ClientDisconnected(IPacket packet, StarboundClient client)
        {

            var cdp = packet as ClientDisconnectPacket;

            if (cdp != null)
            {

                foreach (string channel in Config.Config.Channels)
                {
                    _irc.SendMessage(SendType.Message, channel, String.Format("{0}{1}{0} disconnected", IrcConstants.IrcBold, client.Server.Player.Name));
                }

            }

        }

    }
}

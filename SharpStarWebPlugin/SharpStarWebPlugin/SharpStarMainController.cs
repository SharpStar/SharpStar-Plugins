using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;
using SharpStarWebPlugin.Extensions;
using SharpStarWebPlugin.Responses;
using XSockets.Core.Common.Globals;
using XSockets.Core.Common.Socket;
using XSockets.Core.Common.Socket.Event.Arguments;
using XSockets.Core.XSocket;
using XSockets.Plugin.Framework;
using XSockets.Core.XSocket.Helpers;

namespace SharpStarWebPlugin
{
    [XSocketMetadata("SharpStarMainController", Constants.GenericTextBufferSize, PluginRange.Internal)]
    public class SharpStarMainController : XSocketController
    {

        public readonly static List<StarboundServerClient> ServerClients = new List<StarboundServerClient>();

        public static readonly SharpStarController Controller = new SharpStarController();

        static SharpStarMainController()
        {

            SharpStarMain.Instance.Server.ClientConnected += Global_Server_ClientConnected;
            SharpStarMain.Instance.Server.ClientDisconnected += Global_Server_ClientDisconnected;

            foreach (StarboundServerClient client in SharpStarMain.Instance.Server.Clients)
            {
                client.PlayerClient.PacketReceived += PlayerClient_PacketReceived;
                client.ServerClient.PacketReceived += ServerClient_PacketReceived;
            }

            Controller.OnOpen += Controller_OnOpen;

        }

        private static void Controller_OnOpen(object sender, OnClientConnectArgs e)
        {
        }

        private static void Global_Server_ClientConnected(object sender, ClientConnectedEventArgs e)
        {

            var client = e.ServerClient;

            if (!ServerClients.Contains(client))
                ServerClients.Add(client);

            client.PlayerClient.PacketReceived += PlayerClient_PacketReceived;
            client.ServerClient.PacketReceived += ServerClient_PacketReceived;

        }

        private static void Global_Server_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {

            var client = e.ServerClient;

            ServerClients.Remove(client);

            client.PlayerClient.PacketReceived -= PlayerClient_PacketReceived;
            client.ServerClient.PacketReceived -= ServerClient_PacketReceived;

            if (client.Player != null)
            {
                Controller.SendToAll(client.Player.Name, "playerDisconnected");
            }

        }

        private static void ServerClient_PacketReceived(object sender, PacketReceivedEventArgs e)
        {

            var sc = (StarboundClient)sender;
            var packet = e.Packet;

            if (packet is ConnectionResponsePacket)
            {

                ConnectionResponsePacket ccp = (ConnectionResponsePacket)packet;

                if (ccp.Success)
                {

                    ServerClients.Add(sc.Server);

                    if (sc.Server.Player != null)
                    {

                        Controller.SendToAll(sc.Server.Player.Name, "playerJoined");

                        if (sc.Server.Player.UserAccount != null)
                        {

                            var obj = new
                            {
                                Username = sc.Server.Player.UserAccount.Username,
                                PlayerName = sc.Server.Player.Name
                            };

                            Controller.SendToAll(obj, "userAuthenticated");

                        }

                    }

                }

            }

        }

        private static void PlayerClient_PacketReceived(object sender, PacketReceivedEventArgs e)
        {

            StarboundClient client = (StarboundClient)sender;

            IPacket packet = e.Packet;

            if (packet is ChatSentPacket)
            {

                ChatSentPacket csp = (ChatSentPacket)packet;

                if (client.Server.Player != null)
                {

                    var obj = new
                    {
                        Name = client.Server.Player.Name,
                        Message = csp.Message,
                        Time = DateTime.Now.ToUnixTimeStamp()
                    };

                    Controller.SendToAll(obj, "chatSent");

                }

            }

        }

    }
}

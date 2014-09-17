using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.Db;
using EssentialCommandsPlugin.DbModels;
using NHibernate.Linq;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class ShipCommand
    {

        [Command("ship", "Set your ship's protection status")]
        public async Task Ship(SharpStarClient client, string[] args)
        {

            if (client.Server.Player.UserAccount == null)
            {

                client.SendChatMessage("Server", "You must be logged in to do this!");

                return;

            }

            if (args.Length == 0)
                return;

            string cmd = args[0];

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (args.Length == 1)
                    {

                        Ship ship = session.Query<Ship>().SingleOrDefault(p => p.OwnerUserAccountId == client.Server.Player.UserAccount.Id);

                        if (ship == null)
                        {
                            ship = new Ship
                            {
                                OwnerUserAccountId = client.Server.Player.UserAccount.Id
                            };

                            session.Save(ship);
                        }

                        if (cmd.Equals("public", StringComparison.OrdinalIgnoreCase))
                        {

                            ship.Public = true;

                            session.SaveOrUpdate(ship);

                            client.SendChatMessage("Server", "Your ship is now public!");

                        }
                        else if (cmd.Equals("private", StringComparison.OrdinalIgnoreCase))
                        {
                            ship.Public = false;

                            session.SaveOrUpdate(ship);

                            client.SendChatMessage("Server", "Your ship is now private!");

                            foreach (var cl in SharpStarMain.Instance.Server.Clients)
                            {
                                if (!string.IsNullOrEmpty(cl.Player.PlayerShip) && cl.Player.PlayerShip.Equals(client.Server.Player.Name, StringComparison.OrdinalIgnoreCase))
                                {

                                    if (cl.Player.UserAccount == null)
                                    {

                                        cl.PlayerClient.SendChatMessage("Server", "This ship is now private. Goodbye.");

                                        await cl.ServerClient.WarpTo(cl.Player);

                                        continue;

                                    }

                                    if (cl.Player.UserAccount.Id == ship.OwnerUserAccountId || EssentialCommands.IsAdmin(cl.PlayerClient))
                                        continue;

                                    ShipUser shipUser = session.Query<ShipUser>().SingleOrDefault(p => p.Ship.Id == ship.Id && p.UserAccountId == client.Server.Player.UserAccount.Id);

                                    if (shipUser == null || !shipUser.HasAccess)
                                    {
                                        cl.PlayerClient.SendChatMessage("Server", "This ship is now private. Goodbye.");

                                        await cl.ServerClient.WarpTo(cl.Player);
                                    }

                                }
                            }

                        }
                        else
                        {
                            client.SendChatMessage("Server", "Syntax: /ship <public/private>");
                        }

                    }
                    else if (args.Length == 2)
                    {

                        var usr = SharpStarMain.Instance.Database.GetUser(args[1]);

                        if (usr == null)
                        {
                            client.SendChatMessage("Server", "That user does not exist!");

                            return;
                        }

                        Ship ship = session.Query<Ship>().SingleOrDefault(p => p.OwnerUserAccountId == client.Server.Player.UserAccount.Id);

                        if (ship == null)
                        {
                            ship = new Ship
                            {
                                OwnerUserAccountId = client.Server.Player.UserAccount.Id,
                                Public = false
                            };

                            session.Save(ship);
                        }

                        if (cmd.Equals("add", StringComparison.OrdinalIgnoreCase))
                        {
                            session.Save(new ShipUser
                            {
                                HasAccess = true,
                                Ship = ship,
                                UserAccountId = usr.Id
                            });

                            client.SendChatMessage("Server", String.Format("The user {0} is now allowed on your ship!", args[1]));

                        }
                        else if (cmd.Equals("remove", StringComparison.OrdinalIgnoreCase))
                        {

                            ShipUser shipUser = session.Query<ShipUser>().SingleOrDefault(p => p.UserAccountId == usr.Id && p.Ship.Id == ship.Id);

                            session.Delete(shipUser);

                            client.SendChatMessage("Server", String.Format("The user {0} is no longer allowed on your ship!", args[1]));

                            foreach (var cl in SharpStarMain.Instance.Server.Clients)
                            {
                                if (!string.IsNullOrEmpty(cl.Player.PlayerShip) && cl.Player.PlayerShip.Equals(client.Server.Player.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    cl.PlayerClient.SendChatMessage("Server", "You are no longer allowed on this ship. Goodbye.");

                                    await cl.ServerClient.WarpTo(cl.Player);
                                }
                            }

                        }
                        else
                        {
                            client.SendChatMessage("Server", "Syntax: /ship <add/remove> <username>");
                        }

                    }
                    else
                    {
                        client.SendChatMessage("Server", "Invalid syntax!");
                    }

                    transaction.Commit();

                }
            }
        }

        [PacketEvent(KnownPacket.WarpCommand)]
        public async Task WarpCommandRecv(IPacket packet, SharpStarClient client)
        {

            WarpCommandPacket wcp = (WarpCommandPacket)packet;

            if (wcp.WarpType == WarpType.WarpOtherShip)
            {

                string playerName = wcp.Player;

                var players = SharpStarMain.Instance.Server.Clients.ToList().Where(p => p.Player != null && p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

                using (var session = EssentialsDb.CreateSession())
                {
                    foreach (var plr in players)
                    {

                        if (plr.Player.UserAccount == null)
                            continue;

                        Ship ship = session.Query<Ship>().SingleOrDefault(p => p.OwnerUserAccountId == plr.Player.UserAccount.Id);

                        if (ship == null)
                            continue;

                        if (ship.Public)
                            continue;

                        if (client.Server.Player.UserAccount == null)
                        {
                            client.SendChatMessage("Server", "This ship is private! Warp blocked.");

                            packet.Ignore = true;

                            break;
                        }

                        if (client.Server.Player.UserAccount.Id == ship.OwnerUserAccountId)
                            continue;

                        ShipUser shipUser = session.Query<ShipUser>().SingleOrDefault(p => p.UserAccountId == client.Server.Player.UserAccount.Id && p.Ship.Id == ship.Id);

                        if ((shipUser == null || !shipUser.HasAccess) && !client.Server.Player.UserAccount.IsAdmin)
                        {
                            client.SendChatMessage("Server", "This ship is private! Warp blocked.");

                            packet.Ignore = true;

                            break;
                        }
                    }
                }
            }
        }
    }
}

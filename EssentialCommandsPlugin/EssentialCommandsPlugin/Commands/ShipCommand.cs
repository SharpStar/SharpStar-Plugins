using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class ShipCommand
    {

        [Command("ship")]
        public void Ship(StarboundClient client, string[] args)
        {

            if (client.Server.Player.UserAccount == null)
            {

                client.SendChatMessage("Server", "You must be logged in to do this!");

                return;

            }

            if (args.Length == 0)
                return;

            string cmd = args[0];

            if (args.Length == 1)
            {

                EssentialCommandsShip ship = EssentialCommands.Database.GetShip(client.Server.Player.UserAccount.Id);

                if (ship == null)
                {
                    ship = EssentialCommands.Database.AddShip(client.Server.Player.UserAccount.Id);
                }

                if (cmd.Equals("public", StringComparison.OrdinalIgnoreCase))
                {

                    ship.Public = true;
                    EssentialCommands.Database.UpdateShip(ship);

                    client.SendChatMessage("Server", "Your ship is now public!");

                }
                else if (cmd.Equals("private", StringComparison.OrdinalIgnoreCase))
                {

                    ship.Public = false;
                    EssentialCommands.Database.UpdateShip(ship);

                    client.SendChatMessage("Server", "Your ship is now private!");

                    foreach (var cl in SharpStarMain.Instance.Server.Clients)
                    {
                        if (!string.IsNullOrEmpty(cl.Player.PlayerShip) && cl.Player.PlayerShip.Equals(client.Server.Player.Name, StringComparison.OrdinalIgnoreCase))
                        {

                            if (cl.Player.UserAccount == null)
                            {

                                cl.PlayerClient.SendChatMessage("Server", "This ship is now private. Goodbye.");

                                cl.ServerClient.WarpTo(cl.Player);

                                continue;

                            }

                            if (cl.Player.UserAccount.Id == ship.OwnerUserAccountId)
                                continue;

                            EssentialCommandsShipUser user = EssentialCommands.Database.GetShipUser(cl.Player.UserAccount.Id, ship.Id);

                            if (user == null || !user.HasAccess)
                            {
                                cl.PlayerClient.SendChatMessage("Server", "This ship is now private. Goodbye.");

                                cl.ServerClient.WarpTo(cl.Player);
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

                EssentialCommandsShip ship = EssentialCommands.Database.GetShip(client.Server.Player.UserAccount.Id);

                if (ship == null)
                {
                    ship = EssentialCommands.Database.AddShip(client.Server.Player.UserAccount.Id, false);
                }

                if (cmd.Equals("add", StringComparison.OrdinalIgnoreCase))
                {

                    EssentialCommands.Database.AddShipUser(usr.Id, ship.Id);

                    client.SendChatMessage("Server", String.Format("The user {0} is now allowed on your ship!", args[1]));

                }
                else if (cmd.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {

                    EssentialCommands.Database.RemoveShipUser(usr.Id, ship.Id);

                    client.SendChatMessage("Server", String.Format("The user {0} is no longer allowed on your ship!", args[1]));

                    foreach (var cl in SharpStarMain.Instance.Server.Clients)
                    {
                        if (!string.IsNullOrEmpty(cl.Player.PlayerShip) && cl.Player.PlayerShip.Equals(client.Server.Player.Name, StringComparison.OrdinalIgnoreCase))
                        {

                            cl.PlayerClient.SendChatMessage("Server", "You are no longer allowed on this ship. Goodbye.");

                            cl.ServerClient.WarpTo(cl.Player);

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

        }

        [Event("warpCommand")]
        public void WarpCommandRecv(IPacket packet, StarboundClient client)
        {

            WarpCommandPacket wcp = (WarpCommandPacket)packet;

            if (wcp.WarpType == WarpType.WarpOtherShip)
            {

                string playerName = wcp.Player;

                var players = SharpStarMain.Instance.Server.Clients.Where(p => p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

                foreach (var plr in players)
                {

                    if (plr.Player.UserAccount == null)
                        continue;

                    var ship = EssentialCommands.Database.GetShip(plr.Player.UserAccount.Id);

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

                    var shipUser = EssentialCommands.Database.GetShipUser(client.Server.Player.UserAccount.Id, ship.Id);

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

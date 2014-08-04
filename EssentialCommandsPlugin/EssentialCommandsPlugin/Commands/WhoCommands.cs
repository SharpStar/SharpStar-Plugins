using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class WhoCommands
    {

        [Command("who", "List the players on the server")]
        public void WhoCommand(StarboundClient client, string[] args)
        {

            const int maxPerPage = 4;

            string matchPlayers = String.Empty;

            int page = 0;

            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out page))
                    page--;
                else
                    page = 0;
            }
            else if (args.Length == 2)
            {

                if (int.TryParse(args[0], out page))
                    page--;
                else
                    page = 0;

                matchPlayers = string.Join("", string.Join(" ", args.Skip(1)));

            }

            if (args.Length == 0 || args.Length == 1)
            {

                var listOfPlayers = (from p in SharpStarMain.Instance.Server.Clients where p.Player != null
                                     orderby p.ConnectionTime descending
                                     select new
                                     {
                                         Name = p.Player.Name,
                                         UserAccount = p.Player.UserAccount
                                     }).Skip(page * maxPerPage).Take(maxPerPage);

                client.SendChatMessage("Server", String.Format("Players online (page {0})", page + 1));

                foreach (var player in listOfPlayers)
                {
                    if (player.UserAccount == null)
                        client.SendChatMessage("Server", player.Name);
                    else
                        client.SendChatMessage("Server", String.Format("{0} ({1})", player.Name, player.UserAccount.Username));
                }

            }
            else if (args.Length == 2)
            {

                var listOfPlayers = (from p in SharpStarMain.Instance.Server.Clients
                                     where p.Player != null && p.Player.Name.IndexOf(matchPlayers, StringComparison.OrdinalIgnoreCase) >= 0
                                     orderby p.ConnectionTime descending
                                     select new
                                     {
                                         Name = p.Player.Name,
                                         UserAccount = p.Player.UserAccount
                                     }).Skip(page * maxPerPage).Take(maxPerPage);

                client.SendChatMessage("Server", String.Format("Players online (page {0})", page + 1));

                foreach (var player in listOfPlayers)
                {
                    if (player.UserAccount == null)
                        client.SendChatMessage("Server", player.Name);
                    else
                        client.SendChatMessage("Server", String.Format("{0} ({1})", player.Name, player.UserAccount.Username));
                }

            }
            else
            {
                client.SendChatMessage("Server", "Syntax: /who <page> or /who <page> <player> ");
            }

        }

        [Command("worldwho", "List the players on your world")]
        public void WorldWhoCommand(StarboundClient client, string[] args)
        {

            if (client.Server.Player.Coordinates == null || client.Server.Player.OnShip)
            {

                client.SendChatMessage("Server", "You are not on a world!");

                return;

            }

            const int maxPerPage = 4;

            string matchPlayers = String.Empty;

            int page = 0;

            if (args.Length == 1)
            {
                int.TryParse(args[0], out page);
                page--;
            }
            else if (args.Length == 2)
            {

                int.TryParse(args[0], out page);
                page--;

                matchPlayers = string.Join("", string.Join(" ", args.Skip(1)));

            }

            if (args.Length == 0 || args.Length == 1)
            {

                var listOfPlayers = (from p in SharpStarMain.Instance.Server.Clients
                                     where p.Player != null && p.Player.Coordinates == client.Server.Player.Coordinates
                                     orderby p.ConnectionTime descending
                                     select new
                                     {
                                         Name = p.Player.Name,
                                         UserAccount = p.Player.UserAccount
                                     }).Skip(page * maxPerPage).Take(maxPerPage);

                client.SendChatMessage("Server", String.Format("Players online (page {0})", page + 1));

                foreach (var player in listOfPlayers)
                {
                    if (player.UserAccount == null)
                        client.SendChatMessage("Server", player.Name);
                    else
                        client.SendChatMessage("Server", String.Format("{0} ({1})", player.Name, player.UserAccount.Username));
                }

            }
            else if (args.Length == 2)
            {

                var listOfPlayers = (from p in SharpStarMain.Instance.Server.Clients
                                     where p.Player != null && p.Player.Name.IndexOf(matchPlayers, StringComparison.OrdinalIgnoreCase) >= 0 && p.Player.Coordinates == client.Server.Player.Coordinates
                                     orderby p.ConnectionTime descending
                                     select new
                                     {
                                         Name = p.Player.Name,
                                         UserAccount = p.Player.UserAccount
                                     }).Skip(page * maxPerPage).Take(maxPerPage);

                client.SendChatMessage("Server", String.Format("Players on world (page {0})", page + 1));

                foreach (var player in listOfPlayers)
                {
                    if (player.UserAccount == null)
                        client.SendChatMessage("Server", player.Name);
                    else
                        client.SendChatMessage("Server", String.Format("{0} ({1})", player.Name, player.UserAccount.Username));
                }

            }
            else
            {
                client.SendChatMessage("Server", "Syntax: /worldwho <page> or /worldwho <page> <player> ");
            }

        }

    }
}

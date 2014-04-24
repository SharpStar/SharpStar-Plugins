using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class PermissionCommands
    {

        [Command("addperm")]
        public void AddPermission(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /addperm <permission> <username>");

                return;

            }

            string perm = args[0];
            string userName = args[1];

            var user = SharpStarMain.Instance.Database.GetUser(userName);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            SharpStarMain.Instance.Database.AddPlayerPermission(user.Id, perm, true);

        }

        [Command("removeperm")]
        public void RemovePermission(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /removeperm <permission> <username>");

                return;

            }

            string perm = args[0];
            string userName = args[1];

            var user = SharpStarMain.Instance.Database.GetUser(userName);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            SharpStarMain.Instance.Database.DeletePlayerPermission(user.Id, perm);

        }

    }
}

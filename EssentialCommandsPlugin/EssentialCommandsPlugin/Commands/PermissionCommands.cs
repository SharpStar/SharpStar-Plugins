using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Database;
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

            client.SendChatMessage("Server", "Permission removed from the player");

        }

        [Command("creategroup")]
        public void CreateGroup(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /creategroup <group name> <default>");

            }

            bool defaultGroup = args.Length == 2 && args[1].Equals("true", StringComparison.OrdinalIgnoreCase);

            SharpStarGroup group = SharpStarMain.Instance.Database.CreateGroup(args[0], defaultGroup);

            if (group == null)
            {
                client.SendChatMessage("Server", "Group already exists!");
            }
            else
            {
                client.SendChatMessage("Server", "Group has been created!");
            }

        }

        [Command("deletegroup")]
        public void DeleteGroup(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /deletegroup <group name>");

                return;

            }

            SharpStarGroup group = SharpStarMain.Instance.Database.GetGroup(args[0]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }

            bool added = SharpStarMain.Instance.Database.DeleteGroup(group.Id);

            if (!added)
            {
                client.SendChatMessage("Server", "The group already has that permission!");
            }
            else
            {
                client.SendChatMessage("Server", "Group Deleted!");
            }

        }

        [Command("addgroupperm")]
        public void AddGroupPermission(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /addgroupperm <group name> <permission>");

                return;

            }

            bool exists = SharpStarMain.Instance.Database.AddGroupPermission(args[0], args[1]);

            if (!exists)
            {
                client.SendChatMessage("Server", "Group does not exist!");
            }
            else
            {
                client.SendChatMessage("Server", "Permission added to group!");
            }

        }

        [Command("removegroupperm")]
        public void RemoveGroupPermission(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /removegroupperm <group name> <permission>");

                return;

            }

            bool exists = SharpStarMain.Instance.Database.RemoveGroupPermission(args[0], args[1]);

            if (!exists)
            {
                client.SendChatMessage("Server", "Group does not exist!");
            }
            else
            {
                client.SendChatMessage("Server", "Permission removed from group!");
            }

        }

        [Command("setdefaultgroup")]
        public void SetDefaultGroup(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /setdefaultgroup <group name>");

                return;

            }

            bool exists = SharpStarMain.Instance.Database.SetDefaultGroup(args[0]);

            if (!exists)
            {
                client.SendChatMessage("Server", "Group does not exist!");
            }
            else
            {
                client.SendChatMessage("Server", "Default group set!");
            }

        }

        [Command("setusergroup")]
        public void SetUserGroup(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("permissions"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /setusergroup <username> <group name>");

                return;

            }

            SharpStarGroup group = SharpStarMain.Instance.Database.GetGroup(args[1]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }
            
            SharpStarUser user = SharpStarMain.Instance.Database.GetUser(args[0]);

            SharpStarMain.Instance.Database.ChangeUserGroup(user.Id, group.Id);

            client.SendChatMessage("Server", "User group changed!");

        }

    }
}

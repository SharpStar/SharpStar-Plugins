using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Database;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class PermissionCommands
    {

        public static readonly List<EssentialCommandsGroup> Groups;

        static PermissionCommands()
        {
            Groups = new List<EssentialCommandsGroup>();
        }

        public PermissionCommands()
        {
            RefreshGroups();
        }

        public void RefreshGroups()
        {
            Groups.Clear();
            Groups.AddRange(EssentialCommands.Database.GetGroups());
        }

        [Event("chatReceived")]
        public void OnChatReceived(IPacket packet, SharpStarClient client)
        {

            ChatReceivedPacket csp = (ChatReceivedPacket)packet;

            var plr = SharpStarMain.Instance.Server.Clients.SingleOrDefault(p => p.Player != null && p.Player.Name == csp.Name);

            if (plr != null && plr.Player.UserGroupId.HasValue)
            {
                EssentialCommandsGroup group = Groups.SingleOrDefault(p => p.GroupId == plr.Player.UserGroupId);

                if (group != null && !string.IsNullOrEmpty(group.Prefix))
                    csp.Name = String.Format("[{0}] {1}", group.Prefix, csp.Name);
            }

        }

        [Event("connectionResponse")]
        public void OnConnect(IPacket packet, SharpStarClient client)
        {

            ConnectionResponsePacket crp = (ConnectionResponsePacket)packet;

            foreach (EssentialCommandsGroup group in Groups)
            {
                if (!string.IsNullOrEmpty(group.Prefix) && client.Server.Player.Name.IndexOf(group.Prefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    crp.Success = false;
                    crp.RejectionReason = "Your player name is invalid!";

                    break;
                }
            }

        }

        [Command("addperm", "Give a user permissions to a set of commands")]
        [CommandPermission("permissions")]
        public void AddPermission(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "addperm"))
                return;

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

        [Command("removeperm", "Remove a user's permission")]
        [CommandPermission("permissions")]
        public void RemovePermission(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "removeperm"))
                return;

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

        [Command("creategroup", "Create a permission group")]
        [CommandPermission("permissions")]
        public void CreateGroup(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "creategroup"))
                return;

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /creategroup <group name> <default>");

                return;

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

        [Command("setgroupprefix", "Sets a group's prefix")]
        [CommandPermission("permissions")]
        public void SetGroupPrefix(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setgroupprefix"))
                return;

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /setgroupprefix <group name> <prefix>");

                return;

            }

            SharpStarGroup group = SharpStarMain.Instance.Database.GetGroup(args[0]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }

            EssentialCommandsGroup grp = EssentialCommands.Database.GetGroup(group.Id);

            if (grp == null)
            {
                EssentialCommands.Database.AddGroup(group.Id, args[1]);
            }
            else
            {
                EssentialCommands.Database.SetGroupPrefix(group.Id, args[1]);
            }

            RefreshGroups();

            client.SendChatMessage("Server", "Group prefix set!");


        }

        [Command("deletegroup", "Delete a permission group")]
        [CommandPermission("permissions")]
        public void DeleteGroup(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "deletegroup"))
                return;

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

        [Command("addgroupperm", "Add a permission to a group")]
        [CommandPermission("permissions")]
        public void AddGroupPermission(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "addgroupperm"))
                return;

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

        [Command("setgroupcmdlimit")]
        [CommandPermission("permissions")]
        public void SetGroupCommandLimit(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setgroupcmdlimit"))
                return;

            if (args.Length < 3)
            {

                client.SendChatMessage("Server", "Syntax: /setgroupcmdlimit <group name> <command name> <limit>");

                return;

            }

            SharpStarGroup group = SharpStarMain.Instance.Database.GetGroup(args[0]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }

            int limit;

            if (!int.TryParse(args[2], out limit))
            {

                client.SendChatMessage("Server", "Invalid limit!");

                return;

            }

            EssentialCommandsCommand cmd = EssentialCommands.Database.AddCommand(group.Id, args[1], limit);

            if (cmd != null)
            {
                EssentialCommands.Database.SetCommandLimit(group.Id, args[1], limit);
            }

            client.SendChatMessage("Server", "Limit set!");

        }

        [Command("delgroupcmdlimit")]
        [CommandPermission("permissions")]
        public void RemoveGroupCommandLimit(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "delgroupcmdlimit"))
                return;

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /delgroupcmdlimit <group name> <command name>");

                return;

            }

            SharpStarGroup group = SharpStarMain.Instance.Database.GetGroup(args[0]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }

            if (EssentialCommands.Database.SetCommandLimit(group.Id, args[1], null))
            {
                client.SendChatMessage("Server", "Limit removed!");
            }
            else
            {
                client.SendChatMessage("Server", "There was no limit associated with this command");
            }

        }

        [Command("removegroupperm", "Remove a permission from a group")]
        [CommandPermission("permissions")]
        public void RemoveGroupPermission(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "removegroupperm"))
                return;

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

        [Command("setplanetlimit")]
        [CommandPermission("permissions")]
        public void SetPlanetLimit(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setplanetlimit"))
                return;

            if (args.Length < 2)
            {

                client.SendChatMessage("Server", "Syntax: /setplanetlimit <group name> <limit>");

                return;

            }

            var group = SharpStarMain.Instance.Database.GetGroup(args[0]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }

            int limit;

            if (!int.TryParse(args[1], out limit))
            {

                client.SendChatMessage("Server", "Invalid limit!");

                return;

            }

            EssentialCommands.Database.SetGroupPlanetLimit(group.Id, limit);

            client.SendChatMessage("Server", "Limit has been set!");

        }

        [Command("delplanetlimit")]
        [CommandPermission("permissions")]
        public void RemovePlanetLimit(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setplanetlimit"))
                return;

            if (args.Length != 1)
            {

                client.SendChatMessage("Server", "Syntax: /delplanetlimit <group name>");

                return;

            }

            var group = SharpStarMain.Instance.Database.GetGroup(args[0]);

            if (group == null)
            {

                client.SendChatMessage("Server", "Group does not exist!");

                return;

            }

            EssentialCommands.Database.SetGroupPlanetLimit(group.Id, null);

            client.SendChatMessage("Server", "Limit has been removed!");

        }

        [Command("setdefaultgroup", "Set the default group")]
        [CommandPermission("permissions")]
        public void SetDefaultGroup(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setdefaultgroup"))
                return;

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

        [Command("setusergroup", "Set a user's group")]
        [CommandPermission("permissions")]
        public void SetUserGroup(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setusergroup"))
                return;

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

            if (user == null)
            {

                client.SendChatMessage("Server", "User does not exist!");

                return;

            }

            SharpStarMain.Instance.Database.ChangeUserGroup(user.Id, group.Id);
            EssentialCommands.Database.RemoveUserCommmands(user.Id);

            client.SendChatMessage("Server", "User group changed!");

        }

    }
}

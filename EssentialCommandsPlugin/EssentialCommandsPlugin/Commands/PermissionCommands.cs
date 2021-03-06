﻿using System;
using System.Collections.Generic;
using System.Linq;
using EssentialCommandsPlugin.DbModels;
using NHibernate.Linq;
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

        public static List<Group> Groups;

        static PermissionCommands()
        {
            Groups = new List<Group>();
        }

        public PermissionCommands()
        {
            RefreshGroups();
        }

        public void RefreshGroups()
        {
            using (var session = EssentialsDb.CreateSession())
            {
                Groups = session.CreateCriteria<Group>().List<Group>().ToList();
            }
        }

        [PacketEvent(KnownPacket.ChatReceived)]
        public void OnChatReceived(IPacket packet, SharpStarClient client)
        {
            ChatReceivedPacket csp = (ChatReceivedPacket)packet;

            var plr = SharpStarMain.Instance.Server.Clients.FirstOrDefault(p => p.Player != null && p.Player.ClientId == csp.ClientId);

            if (plr != null && plr.Player.UserAccount != null && plr.Player.UserAccount.Group != null)
            {
                Group group = Groups.SingleOrDefault(p => p.GroupId == plr.Player.UserAccount.Group.Id);

                if (group != null && !string.IsNullOrEmpty(group.Prefix))
                    csp.Name = String.Format("[{0}] {1}", group.Prefix, csp.Name);
            }
        }

        [PacketEvent(KnownPacket.ConnectionResponse)]
        public void OnConnect(IPacket packet, SharpStarClient client)
        {
            if (client.Server.Player == null)
                return;

            ConnectionResponsePacket crp = (ConnectionResponsePacket)packet;

            foreach (Group group in Groups.ToList())
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

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Group grp = session.Query<Group>().SingleOrDefault(p => p.GroupId == group.Id);

                    if (grp == null)
                    {
                        Group newGroup = new Group
                        {
                            Prefix = args[1],
                            GroupId = group.Id
                        };

                        session.Save(newGroup);
                    }
                    else
                    {
                        grp.Prefix = args[1];

                        session.SaveOrUpdate(grp);
                    }

                    transaction.Commit();
                }
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

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Group newGroup = session.Query<Group>().SingleOrDefault(p => p.GroupId == group.Id);

                    if (newGroup != null)
                    {
                        Command cmd = newGroup.Commands.SingleOrDefault(p => p.CommandName.Equals(args[1], StringComparison.OrdinalIgnoreCase));

                        if (cmd == null)
                        {
                            Command newCmd = new Command
                            {
                                CommandName = args[1],
                                CommandLimit = limit,
                                GroupId = group.Id,
                                Group = newGroup
                            };

                            session.Save(newCmd);
                        }
                        else
                        {
                            cmd.CommandLimit = limit;

                            session.SaveOrUpdate(cmd);
                        }
                    }

                    transaction.Commit();

                }
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

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Group grp = session.Query<Group>().SingleOrDefault(p => p.GroupId == group.Id);

                    if (grp != null)
                    {
                        Command cmd = grp.Commands.SingleOrDefault(p => p.CommandName.Equals(args[1], StringComparison.OrdinalIgnoreCase));

                        if (cmd != null)
                        {
                            client.SendChatMessage("Server", "Limit removed!");

                            cmd.CommandLimit = null;

                            session.SaveOrUpdate(cmd);

                            transaction.Commit();
                        }
                        else
                        {
                            client.SendChatMessage("Server", "There was no limit associated with this command");
                        }
                    }
                }
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

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Group grp = session.Query<Group>().SingleOrDefault(p => p.GroupId == group.Id);

                    if (grp != null)
                    {
                        grp.ProtectedPlanetLimit = limit;

                        session.SaveOrUpdate(grp);

                        transaction.Commit();
                    }
                }
            }

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

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Group grp = session.Query<Group>().SingleOrDefault(p => p.GroupId == group.Id);

                    if (grp != null)
                    {
                        grp.ProtectedPlanetLimit = null;

                        session.SaveOrUpdate(grp);

                        transaction.Commit();
                    }
                }
            }

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

            var targetClient = SharpStarMain.Instance.Server.Clients.SingleOrDefault(p => p.Player != null && p.Player.UserAccount != null && p.Player.UserAccount.Id == user.Id);

            if (targetClient != null)
            {
                targetClient.Player.UserAccount = SharpStarMain.Instance.Database.GetUser(user.Id);
            }

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var userCommands = session.Query<UserCommand>().Where(p => p.UserId == user.Id);

                    if (userCommands.Any())
                    {
                        foreach (UserCommand uc in userCommands)
                        {
                            session.Delete(uc);
                        }

                        transaction.Commit();
                    }
                }
            }

            client.SendChatMessage("Server", "User group changed!");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using EssentialCommandsPlugin.DbModels;
using NHibernate.Linq;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class MuteCommand
    {

        private List<Mute> _mutedUsers;

        public MuteCommand()
        {
            RefreshMutedUsers();
        }

        private void RefreshMutedUsers()
        {
            using (var session = EssentialsDb.CreateSession())
            {
                _mutedUsers = session.CreateCriteria<Mute>().List<Mute>().ToList();
            }
        }

        [Command("mute", "Mute a user")]
        [CommandPermission("mute")]
        public void MuteUser(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("mute"))
            {
                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;
            }

            if (args.Length < 2)
            {
                client.SendChatMessage("Server", "Syntax: /mute <username> <time>");

                return;
            }

            var user = SharpStarMain.Instance.Database.GetUser(args[0]);

            if (user == null)
            {
                client.SendChatMessage("Server", "There is no user by that name!");

                return;
            }

            int time;

            if (!int.TryParse(args[1], out time))
            {
                client.SendChatMessage("Server", "Invalid time!");

                return;
            }

            DateTime? expireTime = null;

            if (time > 0)
                expireTime = DateTime.Now.AddMinutes(time);

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (!session.Query<Mute>().Any(p => p.UserId == user.Id))
                    {
                        Mute newMute = new Mute
                        {
                            UserId = user.Id,
                            ExpireTime = expireTime
                        };

                        session.Save(newMute);

                        client.SendChatMessage("Server", String.Format("User {0} has been muted!", user.Username));

                        transaction.Commit();

                        RefreshMutedUsers();
                    }
                    else
                    {
                        client.SendChatMessage("Server", "This user has already been muted!");
                    }
                }
            }

        }

        [Command("unmute", "Unmute a user")]
        [CommandPermission("mute")]
        public void UnmuteUser(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("mute"))
            {
                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;
            }

            var user = SharpStarMain.Instance.Database.GetUser(args[0]);

            if (user == null)
            {
                client.SendChatMessage("Server", "There is no user by that name!");

                return;
            }

            using (var session = EssentialsDb.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Mute mute = session.Query<Mute>().SingleOrDefault(p => p.UserId == user.Id);

                    if (mute != null)
                    {
                        client.SendChatMessage("Server", String.Format("The user {0} has been unmuted.", user.Username));

                        session.Delete(mute);

                        transaction.Commit();

                        RefreshMutedUsers();
                    }
                    else
                    {
                        client.SendChatMessage("Server", String.Format("The user {0} was not muted!", user.Username));
                    }
                }
            }

        }

        [PacketEvent(KnownPacket.ChatSent)]
        public void MonitorChatForMute(IPacket packet, SharpStarClient client)
        {
            if (client.Server != null && client.Server.Player != null && client.Server.Player.UserAccount != null)
            {
                Mute mute = _mutedUsers.SingleOrDefault(p => p.UserId == client.Server.Player.UserAccount.Id);

                if (mute != null)
                {
                    if (mute.ExpireTime <= DateTime.Now)
                    {
                        using (var session = EssentialsDb.CreateSession())
                        {
                            using (var transaction = session.BeginTransaction())
                            {
                                session.Delete(mute);

                                transaction.Commit();
                            }
                        }

                        RefreshMutedUsers();
                    }
                    else
                    {
                        packet.Ignore = true;

                        client.SendChatMessage("Server", "You have been muted!");
                    }
                }
            }
        }

    }
}

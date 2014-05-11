using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssentialCommandsPlugin.Attributes;
using SharpStar.Lib;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class MuteCommand
    {

        private List<EssentialCommandsMute> _mutedUsers;

        public MuteCommand()
        {
            RefreshMutedUsers();
        }

        private void RefreshMutedUsers()
        {
            _mutedUsers = EssentialCommands.Database.GetMutedUsers();
        }

        [Command("mute", "Mute a user")]
        [CommandPermission("mute")]
        public void MuteUser(StarboundClient client, string[] args)
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

            if (EssentialCommands.Database.AddMute(user.Id, expireTime))
            {

                client.SendChatMessage("Server", String.Format("User {0} has been muted!", user.Username));

                RefreshMutedUsers();

            }
            else
            {
                client.SendChatMessage("Server", "This user has already been muted!");
            }

        }

        [Command("unmute", "Unmute a user")]
        [CommandPermission("mute")]
        public void UnmuteUser(StarboundClient client, string[] args)
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

            if (EssentialCommands.Database.RemoveMute(user.Id))
            {

                client.SendChatMessage("Server", String.Format("The user {0} has been unmuted.", user.Username));

                RefreshMutedUsers();

            }
            else
            {
                client.SendChatMessage("Server", String.Format("The user {0} was not muted!", user.Username));
            }

        }

        [Event("chatSent")]
        public void MonitorChatForMute(IPacket packet, StarboundClient client)
        {

            if (client.Server.Player.UserAccount != null)
            {

                EssentialCommandsMute mute = _mutedUsers.SingleOrDefault(p => p.UserId == client.Server.Player.UserAccount.Id);

                if (mute != null)
                {

                    if (mute.ExpireTime <= DateTime.Now)
                    {

                        EssentialCommands.Database.RemoveMute(client.Server.Player.UserAccount.Id);

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

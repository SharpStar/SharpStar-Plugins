using System;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class MakeRemoveAdminCommand
    {

        [Command("makeadmin", "Make a user an administrator")]
        [CommandPermission(true)]
        public void MakeAdmin(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string username = string.Join(" ", args);

            var user = SharpStarMain.Instance.Database.GetUser(username);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            if (user.IsAdmin)
            {

                client.SendChatMessage("Server", "The user is already an admin!");

                return;

            }

            SharpStarMain.Instance.Database.ChangeAdminStatus(user.Id, true);

            client.SendChatMessage("Server", String.Format("{0} is now an admin!", username));

        }

        [Command("removeadmin", "Remove an administrator")]
        [CommandPermission(true)]
        public void RemoveAdmin(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            string username = string.Join(" ", args);

            var user = SharpStarMain.Instance.Database.GetUser(username);

            if (user == null)
            {

                client.SendChatMessage("Server", "The user given does not exist!");

                return;

            }

            if (!user.IsAdmin)
            {

                client.SendChatMessage("Server", "The user is not an admin!");

                return;

            }

            SharpStarMain.Instance.Database.ChangeAdminStatus(user.Id, false);

            client.SendChatMessage("Server", String.Format("{0} is no longer an admin!", username));

        }

    }
}

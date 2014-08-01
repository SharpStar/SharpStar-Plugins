using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Security;
using SharpStar.Lib;
using SharpStar.Lib.Database;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Security;
using SharpStar.Lib.Server;

namespace WebAPIPlugin
{
    public class SharpStarAPI : NancyModule
    {

        public SharpStarAPI()
            : base("/api")
        {

            Get["/players"] = p =>
            {

                var clients = SharpStarMain.Instance.Server.Clients.Where(x => x.Player != null);

                var apiPlayers = new List<APIStarboundPlayer>();

                foreach (StarboundServerClient client in clients)
                {

                    StarboundPlayer player = client.Player;

                    APIStarboundPlayer retPlayer = new APIStarboundPlayer();
                    retPlayer.Name = player.Name;
                    retPlayer.TimeJoined = client.ConnectionTime;

                    if (player.UserAccount != null)
                    {
                        retPlayer.AccountName = player.UserAccount.Username;
                    }

                    apiPlayers.Add(retPlayer);

                }

                return Response.AsJson(apiPlayers);

            };

            Get["/playercount"] = p => Response.AsJson(SharpStarMain.Instance.Server.Clients.Count(x => x.Player != null));


        }

    }

    public class SharpStarAuthAPI : NancyModule
    {



        public SharpStarAuthAPI()
            : base("/authapi")
        {

            this.RequiresAuthentication();

            Get["/accounts"] = p => Response.AsJson(SharpStarMain.Instance.Database.GetUsers().Select(x => x.ToAPIAccount()));

            Get["/account/{username}"] = p =>
            {

                SharpStarUser user = SharpStarMain.Instance.Database.GetUser((string)p.username);

                if (user == null)
                    return Response.AsJson((APISharpStarAccount)null);

                return Response.AsJson(user.ToAPIAccount());

            };

            Post["/createaccount"] = p =>
            {
                string username = Request.Form.Username;
                string password = Request.Form.Password;
                bool admin = Request.Form.Admin;
                int? groupId = Request.Form.GroupId;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return Response.AsJson(new APIResult(false, "A username and a password is required!", 1));

                if (!SharpStarMain.Instance.Database.AddUser(username, password, admin, groupId))
                    return Response.AsJson(new APIResult(false, "An error occurred while registering the account!", 2));

                return Response.AsJson(new APIResult(true));
            };

            Post["/deleteaccount"] = p =>
            {

                string username = Request.Form.Username;

                if (string.IsNullOrEmpty(username))
                    return Response.AsJson(new APIResult(false, "A username is required!", 1));

                if (!SharpStarMain.Instance.Database.DeleteUser(username))
                    return Response.AsJson(new APIResult(false, "Failed to delete the specified user!", 2));

                return Response.AsJson(new APIResult(true));

            };

            Post["/setusergroup"] = p =>
            {

                string username = Request.Form.Username;
                int? groupId = Request.Form.GroupId;

                if (string.IsNullOrEmpty(username) || !groupId.HasValue)
                    return Response.AsJson(new APIResult(false, "A username and group id is required!", 1));

                if (!SharpStarMain.Instance.Database.ChangeUserGroup(username, groupId.Value))
                    return Response.AsJson(new APIResult(false, "An error occurred while changing the user's group!", 2));

                return Response.AsJson(new APIResult(true));

            };

            Post["/changeuserpassword"] = p =>
            {

                string username = Request.Form.Username;
                string newPassword = Request.Form.NewPassword;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
                    return Response.AsJson(new APIResult(false, "You must provide a username and the new password for that user account!", 1));

                if (!SharpStarMain.Instance.Database.UpdateUserPassword(username, newPassword))
                    return Response.AsJson(new APIResult(false, "An error occurred while changing the user's password!"));

                return Response.AsJson(new APIResult(true));

            };

            Post["/validatepassword"] = p =>
            {

                string username = Request.Form.Username;
                string password = Request.Form.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return Response.AsJson(new APIResult(false, "A username and password is required!", 1));

                SharpStarUser user = SharpStarMain.Instance.Database.GetUser(username);

                if (user == null)
                    return Response.AsJson(new APIResult(false, "The user does not exist!", 2));

                bool success = user.Hash == SharpStarSecurity.GenerateHash(user.Username, password, user.Salt, 5000);

                return Response.AsJson(new APIResult(success));

            };

            Post["/executeconsole"] = p =>
            {

                string command = Request.Form.Command;
                string args = Request.Form.Arguments;

                if (!SharpStarMain.Instance.PluginManager.PassConsoleCommand(command, args.Split(' ')))
                    return Response.AsJson(new APIResult(false, "There is no command by that name!"));

                return Response.AsJson(new APIResult(true));

            };

        }

    }

    public class APIResult
    {

        public bool Success { get; set; }

        public int ErrorCode { get; set; }

        public string Error { get; set; }

        public APIResult(bool success, string error = "", int errorCode = -1)
        {
            Success = success;
            Error = error;
            ErrorCode = errorCode;
        }

    }

    public static class APIHelper
    {

        public static APISharpStarAccount ToAPIAccount(this SharpStarUser user, bool online = false)
        {

            APISharpStarAccount retAccount = new APISharpStarAccount();

            retAccount = new APISharpStarAccount();
            retAccount.ID = user.Id;
            retAccount.Username = user.Username;
            retAccount.IsAdmin = user.IsAdmin;
            retAccount.LastLogin = user.LastLogin;
            retAccount.GroupId = user.GroupId;
            retAccount.IsOnline = online;

            return retAccount;

        }

    }

}

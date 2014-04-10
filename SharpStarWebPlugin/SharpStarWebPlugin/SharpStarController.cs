using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SharpStar.Lib;
using SharpStar.Lib.Security;
using SharpStarWebPlugin.Responses;
using XSockets.Core.Common.Socket.Attributes;
using XSockets.Core.Common.Socket.Event.Arguments;
using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Helpers;

namespace SharpStarWebPlugin
{
    public class SharpStarController : XSocketController
    {

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(_username);
            }
        }


        private string _username;

        private bool _isAdmin;

        public SharpStarController()
        {

            _username = String.Empty;
            _isAdmin = false;

            OnOpen += SharpStarController_OnOpen;

        }

        private void SharpStarController_OnOpen(object sender, OnClientConnectArgs e)
        {

            if (this.HasParameterKey("username") && this.HasParameterKey("password"))
            {
                Authenticate(this.GetParameter("username"), this.GetParameter("password"));
            }

        }

        public override bool OnAuthorization(AuthorizeAttribute authorizeAttribute)
        {
            return IsAuthenticated;
        }

        public void Authenticate(string username, string password)
        {

            var user = SharpStarMain.Instance.Database.GetUser(username);

            if (user == null)
            {

                this.SendError(new Exception("Wrong username/password"));

                return;

            }

            string hash = SharpStarSecurity.GenerateHash(username, password, user.Salt, 5000);

            if (hash != user.Hash)
            {

                this.SendError(new Exception("Wrong username/password"));

                return;

            }

            _isAdmin = user.IsAdmin;
            _username = user.Username;

            this.Send(true, "authenticationSuccessful");

        }

        public void Logout()
        {
            _username = String.Empty;
            _isAdmin = false;
        }

        public void GetAllPlayers()
        {

            List<SharpStarPlayer> players = new List<SharpStarPlayer>();

            foreach (var client in SharpStarMain.Instance.Server.Clients)
            {

                if (client.Player != null)
                {

                    SharpStarPlayer plr = new SharpStarPlayer();
                    plr.Name = client.Player.Name;
                    plr.Duration = (int)(DateTime.Now - client.ConnectionTime).TotalMilliseconds;

                    if (client.Player.UserAccount != null)
                    {
                        plr.AuthenticatedUsername = client.Player.UserAccount.Username;
                        plr.IsAdmin = client.Player.UserAccount.IsAdmin;
                    }

                    players.Add(plr);

                }

            }

            this.Send(players, "getAllPlayers");

        }

        [Authorize]
        public void GetUser(string username)
        {

            if (_isAdmin)
            {

                var user = SharpStarMain.Instance.Database.GetUser(username);

                if (user != null)
                    this.Send(new SharpStarUserResponse { Username = user.Username, IsAdmin = user.IsAdmin }, "getUser");
                else
                    this.SendError(new Exception("User does not exist!"));

            }
            else
            {
                this.SendError(new Exception("You do not have permission to do this!"));
            }

        }

    }
}

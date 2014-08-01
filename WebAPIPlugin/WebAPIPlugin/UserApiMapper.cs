using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Security;

namespace WebAPIPlugin
{
    public class UserApiMapper : IUserApiMapper
    {

        public IUserIdentity GetUserFromAccessToken(string accessToken)
        {
            if (accessToken == APIPlugin.Config.ConfigFile.APIKey)
                return new UserIdentity { UserName = "admin" };

            return null;
        }

    }
}

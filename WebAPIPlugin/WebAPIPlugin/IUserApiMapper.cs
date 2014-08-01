using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Security;

namespace WebAPIPlugin
{
    public interface IUserApiMapper
    {
        IUserIdentity GetUserFromAccessToken(string accessToken);
    }
}

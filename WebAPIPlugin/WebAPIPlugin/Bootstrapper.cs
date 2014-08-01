using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace WebAPIPlugin
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            var configuration =
                new StatelessAuthenticationConfiguration(nancyContext =>
                {
                    const string key = "Bearer ";
                    string accessToken = null;

                    if (nancyContext.Request.Headers.Authorization.StartsWith(key))
                    {
                        accessToken = nancyContext.Request.Headers.Authorization.Substring(key.Length);
                    }

                    if (string.IsNullOrWhiteSpace(accessToken))
                        return null;

                    var userValidator = container.Resolve<IUserApiMapper>();

                    return userValidator.GetUserFromAccessToken(accessToken);
                });

            StatelessAuthentication.Enable(pipelines, configuration);

        }
    }
}

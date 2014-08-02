using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Addins;
using Nancy.Hosting.Self;
using SharpStar.Lib.Plugins;
using WebAPIPlugin.Config;

[assembly: Addin("WebAPI", Version = "1.0")]
[assembly: AddinDescription("A Web API for SharpStar")]
[assembly :ImportAddinAssembly("Nancy.dll")]
[assembly: ImportAddinAssembly("Nancy.Hosting.Self.dll")]
[assembly: ImportAddinAssembly("Nancy.Authentication.Stateless.dll")]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]
namespace WebAPIPlugin
{
    [Extension]
    public class APIPlugin : CSPlugin
    {

        public static readonly APIConfig Config = new APIConfig("webapi.json");

        public override string Name
        {
            get { return "Web API Plugin"; }
        }

        private NancyHost _host;

        public override void OnLoad()
        {
            _host = new NancyHost(new Uri("http://localhost:21020"));
            _host.Start();
        }

        public override void OnUnload()
        {
            _host.Dispose();
        }

    }
}

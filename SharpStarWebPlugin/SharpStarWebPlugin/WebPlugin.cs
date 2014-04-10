using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Addins;
using SharpStar.Lib.Plugins;

[assembly: Addin]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]

namespace SharpStarWebPlugin
{
    [Extension]
    public class WebPlugin : CSPlugin
    {

        private readonly SharpStarWeb _web;

        public override string Name
        {
            get { return "Web Plugin"; }
        }

        public WebPlugin()
        {
            _web = new SharpStarWeb();
        }

        public override void OnLoad()
        {
            _web.Start();
        }

        public override void OnUnload()
        {
            _web.Stop();
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SharpStarIrcPlugin
{
    [JsonObject]
    public class IrcConfigFile
    {

        public string Nick { get; set; }

        public string IrcNetwork { get; set; }

        public int IrcPort { get; set; }

        public List<string> Channels { get; set; }

    }
}

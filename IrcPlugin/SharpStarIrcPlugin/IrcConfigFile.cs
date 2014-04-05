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

        [JsonProperty]
        public string Nick { get; set; }

        [JsonProperty]
        public string Password { get; set; }

        [JsonProperty]
        public string CommandPrefix { get; set; }

        [JsonProperty]
        public string IrcNetwork { get; set; }

        [JsonProperty]
        public int IrcPort { get; set; }

        [JsonProperty]
        public List<IrcChannel> Channels { get; set; }

        public IrcConfigFile()
        {
            Channels = new List<IrcChannel>();
        }

    }
}

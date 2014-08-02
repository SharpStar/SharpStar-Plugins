using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SharpStarIrcPlugin
{
    [JsonObject]
    public class IrcChannel
    {

        [JsonProperty]
        public string Channel { get; set; }

        [JsonProperty]
        public string Password { get; set; }

        [JsonProperty]
        public bool EnableDebugOutput { get; set; }
        
        [JsonProperty]
        public bool EnableInfoOutput { get; set; }

        [JsonProperty]
        public bool EnableWarningOuput { get; set; }

        [JsonProperty]
        public bool EnableErrorOutput { get; set; }

        public IrcChannel()
        {
            EnableDebugOutput = false;
            EnableInfoOutput = false;
            EnableWarningOuput = false;
            EnableErrorOutput = false;
        }

    }
}

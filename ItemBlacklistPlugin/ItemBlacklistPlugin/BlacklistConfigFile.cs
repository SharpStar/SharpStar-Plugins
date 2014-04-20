using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ItemBlacklistPlugin
{
    [JsonObject]
    public class BlacklistConfigFile
    {

        [JsonProperty("blacklistedItems")]
        public List<string> BlacklistedItems { get; set; }

        public BlacklistConfigFile()
        {
            BlacklistedItems = new List<string>();
        }

    }
}

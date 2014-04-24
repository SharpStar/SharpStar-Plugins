using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace EssentialCommandsPlugin
{
    [JsonObject]
    public class EssentialCommandsAdvert
    {

        [JsonProperty("advertName")]
        public string AdvertName { get; set; }

        [JsonProperty("advertMessage")]
        public string AdvertMessage { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WebAPIPlugin.Config
{
    [JsonObject]
    public class APIConfigFile
    {

        [JsonProperty("apiKey")]
        public string APIKey { get; set; }

    }
}

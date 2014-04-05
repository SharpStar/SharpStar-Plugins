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

        public string Channel { get; set; }

        public string Password { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ServerManagementPlugin.Config
{
    [JsonObject]
    public class ServerManagementConfigFile
    {

        [JsonProperty]
        public bool AutoRestartOnCrash { get; set; }

        [JsonProperty]
        public int ServerCheckInterval { get; set; }

        [JsonProperty]
        public string ServerExecutable { get; set; }

        [JsonProperty]
        public string ServerRestartMessage { get; set; }

        public ServerManagementConfigFile()
        {
            AutoRestartOnCrash = true;
            ServerCheckInterval = 5;
            ServerExecutable = String.Empty;
            ServerRestartMessage = "A server restart has been initiated. Restarting in:";
        }

    }
}

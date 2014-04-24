using System.Collections.Generic;
using Newtonsoft.Json;

namespace EssentialCommandsPlugin
{
    [JsonObject]
    public class EssentialCommandsConfigFile
    {

        [JsonProperty]
        public string Motd { get; set; }

        [JsonProperty]
        public List<EssentialCommandsAdvert> Adverts { get; set; }

        public EssentialCommandsConfigFile()
        {
            Adverts = new List<EssentialCommandsAdvert>();
        }

    }
}

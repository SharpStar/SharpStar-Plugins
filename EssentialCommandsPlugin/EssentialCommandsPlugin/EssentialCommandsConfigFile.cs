using Newtonsoft.Json;

namespace EssentialCommandsPlugin
{
    [JsonObject]
    public class EssentialCommandsConfigFile
    {

        [JsonProperty]
        public string Motd { get; set; }

    }
}

using System.Collections.Generic;
using Newtonsoft.Json;
using SharpStar.Lib.DataTypes;

namespace EssentialCommandsPlugin
{
    [JsonObject]
    public class EssentialCommandsConfigFile
    {

        [JsonProperty]
        public string Motd { get; set; }

        [JsonProperty]
        public List<EssentialCommandsAdvert> Adverts { get; set; }

        [JsonProperty]
        public WorldCoordinate SpawnCoordinates { get; set; }

        public EssentialCommandsConfigFile()
        {
            Adverts = new List<EssentialCommandsAdvert>();
        }

    }
}

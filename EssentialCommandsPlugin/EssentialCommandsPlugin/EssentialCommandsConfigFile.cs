using System;
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

        [JsonProperty]
        public bool AllowProjectiles { get; set; }

        [JsonProperty]
        public string ReplaceProjectileWith { get; set; }

        [JsonProperty]
        public List<string> ProjectileWhitelist { get; set; }

        [JsonProperty]
        public string MaxTempBanTime { get; set; }

        [JsonProperty]
        public string TempBanTemplate { get; set; }

        [JsonProperty]
        public string PermBanTemplate { get; set; }

        public EssentialCommandsConfigFile()
        {
            Motd = String.Empty;
            Adverts = new List<EssentialCommandsAdvert>();
            AllowProjectiles = false;
            ReplaceProjectileWith = "snowball";
            ProjectileWhitelist = new List<string>();
            MaxTempBanTime = "30d";
            TempBanTemplate = @"You have been banned!
<reason>
Time Left: <time>";
            PermBanTemplate = @"You have been permanently banned from this server!
<reason>";
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SharpStar.Lib.Security;

namespace WebAPIPlugin.Config
{
    public class APIConfig
    {
        public APIConfigFile ConfigFile { get; set; }

        public string FileName { get; set; }

        public APIConfig(string fileName)
        {

            FileName = fileName;

            Reload();

        }

        private void SetDefaults()
        {
            ConfigFile.APIKey = SharpStarSecurity.GenerateSalt();

            Save();
        }

        public void Save()
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(ConfigFile, Formatting.Indented));
        }

        public void Reload()
        {

            if (File.Exists(FileName))
            {
                ConfigFile = JsonConvert.DeserializeObject<APIConfigFile>(File.ReadAllText(FileName));

                if (string.IsNullOrEmpty(ConfigFile.APIKey))
                    SetDefaults();

            }
            else
            {

                ConfigFile = new APIConfigFile();

                SetDefaults();

            }

        }
    }
}

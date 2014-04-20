using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ItemBlacklistPlugin
{
    public class BlacklistConfig
    {

        public BlacklistConfigFile ConfigFile { get; set; }

        public string FileName { get; private set; }

        public BlacklistConfig(string file)
        {

            FileName = file;

            Reload();

        }

        public void SetDefaults()
        {
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
                ConfigFile = JsonConvert.DeserializeObject<BlacklistConfigFile>(File.ReadAllText(FileName));
            }
            else
            {

                ConfigFile = new BlacklistConfigFile();

                SetDefaults();

            }

        }

    }
}

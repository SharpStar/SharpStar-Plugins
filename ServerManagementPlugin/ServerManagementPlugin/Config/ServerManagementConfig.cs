using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ServerManagementPlugin.Config
{
    public class ServerManagementConfig
    {
         public ServerManagementConfigFile ConfigFile { get; set; }

        public string FileName { get; set; }

        public ServerManagementConfig(string fileName)
        {

            FileName = fileName;

            Reload();

        }

        private void SetDefaults()
        {
            ConfigFile.AutoRestartOnCrash = true;
            ConfigFile.ServerCheckInterval = 5;
            ConfigFile.ServerExecutable = String.Empty;

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
                ConfigFile = JsonConvert.DeserializeObject<ServerManagementConfigFile>(File.ReadAllText(FileName));
            }
            else
            {

                ConfigFile = new ServerManagementConfigFile();

                SetDefaults();

            }

        }
    }
}

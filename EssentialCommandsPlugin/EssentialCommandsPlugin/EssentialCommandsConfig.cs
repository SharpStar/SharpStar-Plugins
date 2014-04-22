using System.IO;
using Newtonsoft.Json;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsConfig
    {

        public EssentialCommandsConfigFile ConfigFile { get; set; }

        public string FileName { get; set; }

        public EssentialCommandsConfig(string fileName)
        {

            FileName = fileName;

            Reload();

        }

        private void SetDefaults()
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
                ConfigFile = JsonConvert.DeserializeObject<EssentialCommandsConfigFile>(File.ReadAllText(FileName));
            }
            else
            {

                ConfigFile = new EssentialCommandsConfigFile();

                SetDefaults();

            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SharpStarIrcPlugin
{
    public class IrcConfig
    {

        private static readonly Random Rand = new Random();

        public IrcConfigFile Config { get; set; }

        public string FileName { get; private set; }

        public IrcConfig(string file)
        {

            FileName = file;

            if (File.Exists(file))
            {
                Config = JsonConvert.DeserializeObject<IrcConfigFile>(File.ReadAllText(file));
            }
            else
            {
                
                Config = new IrcConfigFile();
                
                SetDefaults();

            }

        }

        public void SetDefaults()
        {

            Config.Nick = "SharpStarBot-" + Rand.Next(1000);
            Config.Channels = new List<string>();
            Config.IrcNetwork = "irc.freenode.net";
            Config.IrcPort = 6667;

            Save();

        }

        public void Save()
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

    }
}

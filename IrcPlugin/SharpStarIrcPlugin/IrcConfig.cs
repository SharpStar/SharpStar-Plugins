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

            Reload();

        }

        public void SetDefaults()
        {

            Config.Nick = "SharpStarBot-" + Rand.Next(1000);
            Config.Channels.Add(new IrcChannel { Channel = "##sharpstar-bots", Password = null });

            Config.IrcNetwork = "irc.freenode.net";
            Config.IrcPort = 6667;
            Config.CommandPrefix = "!";


            Save();

        }

        public void Save()
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

        public void Reload()
        {

            if (File.Exists(FileName))
            {
                Config = JsonConvert.DeserializeObject<IrcConfigFile>(File.ReadAllText(FileName));
            }
            else
            {

                Config = new IrcConfigFile();

                SetDefaults();

            }

        }

    }
}

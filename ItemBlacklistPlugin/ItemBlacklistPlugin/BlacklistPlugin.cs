using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Addins;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

[assembly: Addin]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]

namespace ItemBlacklistPlugin
{
    [Extension]
    public class BlacklistPlugin : CSPlugin
    {

        private BlacklistConfig _config;

        private List<int> _playersBeingKicked;

        private readonly object _playerLocker = new object();

        public override string Name
        {
            get { return "Item Blacklist"; }
        }

        public override void OnLoad()
        {

            _config = new BlacklistConfig("itemblacklist.json");
            _config.SetDefaults();

            _playersBeingKicked = new List<int>();

            RegisterEvent("clientContextUpdate", ClientContextUpdate);

            RegisterCommand("blacklistitem", BlacklistItem);
            RegisterCommand("unblacklistitem", UnblacklistItem);

        }

        private void BlacklistItem(StarboundClient client, string[] args)
        {

            if (client.Server.Player.HasPermission("blacklistitem") || (client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.IsAdmin))
            {

                string item = args[0];

                if (!_config.ConfigFile.BlacklistedItems.Any(p => p.Equals(item, StringComparison.OrdinalIgnoreCase)))
                {
                    _config.ConfigFile.BlacklistedItems.Add(item);
                    _config.Save();
                }

                client.SendChatMessage("Server", String.Format("{0} has been blacklisted!", item));

            }
            else
            {
                client.SendChatMessage("Server", "You do not have permission to use this command!");
            }

        }

        private void UnblacklistItem(StarboundClient client, string[] args)
        {

            if (client.Server.Player.HasPermission("blacklistitem") || (client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.IsAdmin))
            {

                string item = args[0];

                _config.ConfigFile.BlacklistedItems.RemoveAll(p => p.Equals(item, StringComparison.OrdinalIgnoreCase));
                _config.Save();

                client.SendChatMessage("Server", String.Format("{0} has been removed from the blacklist!", item));

            }
            else
            {
                client.SendChatMessage("Server", "You do not have permission to use this command!");
            }

        }

        private void ClientContextUpdate(IPacket packet, StarboundClient client)
        {

            ClientContextUpdatePacket ccup = (ClientContextUpdatePacket)packet;

            if (ccup.World != null)
            {

                if (client.Server.Player.HasPermission("allowblacklisteditems") || (client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.IsAdmin))
                    return;

                var meta = ccup.World.GetMetadata();
                var variant = (VariantDict)meta.Data.Value;

                var var = (Variant[])variant["playerStart"].Value;

                int x = Convert.ToInt32(var[0].Value) / 32;
                int y = Convert.ToInt32(var[1].Value) / 32;

                var ents = ccup.World.GetEntities((byte)x, (byte)y);

                foreach (var ent in ents)
                {

                    var v = ent.Data.Value;

                    var d = (VariantDict)v;

                    if (d.ContainsKey("items"))
                    {

                        var val = (Variant[])(d["items"].Value);

                        foreach (var z in val)
                        {

                            if (z.Value is VariantDict)
                            {

                                VariantDict c = (VariantDict)z.Value;

                                string name = (string)c["name"].Value;

                                int count = Convert.ToInt32(c["count"].Value);

                                if (_config.ConfigFile.BlacklistedItems.Any(p => p.Equals(name, StringComparison.OrdinalIgnoreCase)) && !_playersBeingKicked.Contains(client.Server.ClientId))
                                {

                                    Console.WriteLine("{0} has a blacklisted item, kicking...", client.Server.Player.Name);

                                    lock (_playerLocker)
                                        _playersBeingKicked.Add(client.Server.ClientId);

                                    Task.Factory.StartNew(() =>
                                    {

                                        client.SendChatMessage("Server", "You have a blacklisted item! You will now be kicked");

                                        client.Server.Disconnected += (sender, e) =>
                                        {
                                            lock (_playerLocker)
                                                _playersBeingKicked.Remove(client.Server.ClientId);
                                        };

                                        Thread.Sleep(3000);

                                        client.Disconnect();

                                    });

                                    return;

                                }

                            }

                        }

                    }

                }

            }

        }

    }
}

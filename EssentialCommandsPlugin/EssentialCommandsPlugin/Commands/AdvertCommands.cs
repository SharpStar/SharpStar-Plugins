using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class AdvertCommands
    {

        private readonly Dictionary<string, Timer> _advertTimers;

        public AdvertCommands()
        {
            _advertTimers = new Dictionary<string, Timer>();
        }

        public void StartSendingAdverts()
        {
            for (int i = 0; i < EssentialCommands.Config.ConfigFile.Adverts.Count; i++)
            {

                EssentialCommandsAdvert advert = EssentialCommands.Config.ConfigFile.Adverts[i];

                if (advert.Enabled)
                {

                    Timer timer = new Timer();
                    timer.Interval = TimeSpan.FromMinutes(advert.Interval).TotalMilliseconds;
                    timer.Elapsed += (sender, e) =>
                    {

                        foreach (var cl in SharpStarMain.Instance.Server.Clients.ToList())
                        {
                            cl.PlayerClient.SendChatMessage("Advert", advert.AdvertMessage);
                        }

                    };

                    _advertTimers.Add(advert.AdvertName, timer);

                }

            }
        }

        public void StopSendingAdverts()
        {
            foreach (var timer in _advertTimers)
            {
                timer.Value.Stop();
                timer.Value.Dispose();
            }

            _advertTimers.Clear();
        }

        [Command("addadvert", "Add an advert to display")]
        [CommandPermission("adverts")]
        public void AddAdvert(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "addadvert"))
                return;

            if (args.Length < 3)
            {

                client.SendChatMessage("Server", "Syntax: /addadvert <name> <interval> <advert>");

                return;

            }

            string name = args[0];
            string intervalStr = args[1];
            string advert = string.Join("", string.Join(" ", args.Skip(2))); //skip name, interval

            int interval;

            if (!int.TryParse(intervalStr, out interval) || interval == 0)
            {

                client.SendChatMessage("Server", "Invalid interval!");

                return;

            }

            if (EssentialCommands.Config.ConfigFile.Adverts.Any(p => p.AdvertName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {

                client.SendChatMessage("Server", "There is already an advert by that name!");

                return;

            }

            var timer = new Timer();
            timer.Interval = TimeSpan.FromMinutes(interval).TotalMilliseconds;
            timer.Elapsed += (sender, e) =>
            {

                foreach (var cl in SharpStarMain.Instance.Server.Clients.ToList())
                {
                    cl.PlayerClient.SendChatMessage("Advert", advert);
                }

            };

            timer.Start();

            _advertTimers.Add(name, timer);

            var adv = new EssentialCommandsAdvert();
            adv.AdvertName = name;
            adv.AdvertMessage = advert;
            adv.Interval = interval;
            adv.Enabled = true;

            EssentialCommands.Config.ConfigFile.Adverts.Add(adv);
            EssentialCommands.Config.Save();

            client.SendChatMessage("Server", "Advert has been added and enabled.");

        }

        [Command("removeadvert", "Remove an advert")]
        [CommandPermission("adverts")]
        public void RemoveAdvert(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "removeadvert"))
                return;

            if (!EssentialCommands.Config.ConfigFile.Adverts.Any(p => p.AdvertName.Equals(args[0], StringComparison.OrdinalIgnoreCase)))
            {

                client.SendChatMessage("Server", "There are no adverts by that name!");

                return;

            }

            var timer = _advertTimers.SingleOrDefault(p => p.Key.Equals(args[0], StringComparison.OrdinalIgnoreCase));

            if (timer.Value != null)
            {

                timer.Value.Stop();
                timer.Value.Dispose();

                _advertTimers.Remove(timer.Key);

            }

            EssentialCommands.Config.ConfigFile.Adverts.RemoveAll(p => p.AdvertName.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            EssentialCommands.Config.Save();

            client.SendChatMessage("Server", "Advert removed!");

        }

        [Command("toggleadvert", "Toggle advert")]
        [CommandPermission("adverts")]
        public void ToggleAdvert(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "toggleadvert"))
                return;

            if (args.Length != 1)
            {

                client.SendChatMessage("Server", "Syntax: /toggleadvert <name>");

                return;

            }

            EssentialCommandsAdvert advert = EssentialCommands.Config.ConfigFile.Adverts.SingleOrDefault(p => p.AdvertName.Equals(args[0], StringComparison.OrdinalIgnoreCase));

            if (advert == null)
            {

                client.SendChatMessage("Server", "There are no adverts by that name!");

                return;

            }

            advert.Enabled = !advert.Enabled;

            EssentialCommands.Config.Save();

            var timer = _advertTimers.SingleOrDefault(p => p.Key.Equals(args[0], StringComparison.OrdinalIgnoreCase));

            if (timer.Value != null)
            {

                if (!advert.Enabled)
                {

                    timer.Value.Stop();
                    timer.Value.Dispose();

                    _advertTimers.Remove(timer.Key);

                }

            }
            else
            {

                var newTimer = new Timer();
                newTimer.Interval = TimeSpan.FromMinutes(advert.Interval).TotalMilliseconds;
                newTimer.Elapsed += (sender, e) =>
                {
                    foreach (var cl in SharpStarMain.Instance.Server.Clients.ToList())
                    {
                        cl.PlayerClient.SendChatMessage("Advert", advert.AdvertMessage);
                    }
                };

                newTimer.Start();

                _advertTimers.Add(advert.AdvertName, newTimer);

            }

            client.SendChatMessage("Server", "Advert is now " + (advert.Enabled ? "enabled" : "disabled"));

        }

    }
}

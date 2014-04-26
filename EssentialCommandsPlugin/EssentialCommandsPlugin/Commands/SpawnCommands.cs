using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class SpawnCommands
    {

        [Command("spawnplanet")]
        public void GoToSpawnPlanet(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("gotospawn"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (EssentialCommands.Config.ConfigFile.SpawnCoordinates == null)
            {

                client.SendChatMessage("Server", "There is currently no spawn planet set!");

                return;

            }

            if (!client.Server.Player.OnShip)
                client.Server.ServerClient.WarpTo(client.Server.Player);

            client.Server.ServerClient.MoveShip(EssentialCommands.Config.ConfigFile.SpawnCoordinates);

            client.SendChatMessage("Server", "Your ship is now moving to the spawn planet!");

        }

        [Command("setspawn")]
        public void SetSpawnPlanet(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("spawn"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            if (client.Server.Player.OnShip)
            {

                client.SendChatMessage("Server", "You are not on a planet!");

                return;

            }

            EssentialCommands.Config.ConfigFile.SpawnCoordinates = client.Server.Player.Coordinates;
            EssentialCommands.Config.Save();

            client.SendChatMessage("Server", "Spawn planet set to your current coordinates!");

        }

        [Command("unsetspawn")]
        public void UnsetSpawnPlanet(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.IsAdmin(client) && !client.Server.Player.HasPermission("spawn"))
            {

                client.SendChatMessage("Server", "You do not have permission to use this command!");

                return;

            }

            EssentialCommands.Config.ConfigFile.SpawnCoordinates = null;
            EssentialCommands.Config.Save();

            client.SendChatMessage("Server", "Spawn planet unset!");

        }

    }
}

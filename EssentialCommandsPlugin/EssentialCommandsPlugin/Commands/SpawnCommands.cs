using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class SpawnCommands
    {

        [Command("spawnplanet", "Go to the spawn planet")]
        [CommandPermission("gotospawn")]
        public void GoToSpawnPlanet(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "spawnplanet"))
                return;

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

        [Command("setspawn", "Set the spawn location")]
        [CommandPermission("spawn")]
        public void SetSpawnPlanet(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "setspawn"))
                return;

            if (client.Server.Player.OnShip)
            {

                client.SendChatMessage("Server", "You are not on a planet!");

                return;

            }

            EssentialCommands.Config.ConfigFile.SpawnCoordinates = client.Server.Player.Coordinates;
            EssentialCommands.Config.Save();

            client.SendChatMessage("Server", "Spawn planet set to your current coordinates!");

        }

        [Command("unsetspawn", "Unset the spawn location")]
        [CommandPermission("spawn")]
        public void UnsetSpawnPlanet(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "unsetspawn"))
                return;

            EssentialCommands.Config.ConfigFile.SpawnCoordinates = null;
            EssentialCommands.Config.Save();

            client.SendChatMessage("Server", "Spawn planet unset!");

        }

    }
}

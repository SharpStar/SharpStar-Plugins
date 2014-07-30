using System;
using System.Collections.Generic;
using System.Linq;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Database;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class ProtectPlanetCommands
    {

        private Dictionary<EssentialCommandsPlanet, List<EssentialCommandsBuilder>> _planets;

        public ProtectPlanetCommands()
        {
            RefreshProtectedPlanets();
        }

        [Command("protect", "Protect a planet")]
        [CommandPermission("planetprotect")]
        public void Protect(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "protect"))
                return;

            if (client.Server.Player.OnShip || client.Server.Player.Coordinates == null)
            {

                client.SendChatMessage("Server", "You are not on a planet!");

                return;

            }

            EssentialCommandsPlanet planet = EssentialCommands.Database.GetProtectedPlanet(client.Server.Player.Coordinates);

            if (planet != null)
            {

                client.SendChatMessage("Server", "Planet is already protected!");

                return;

            }

            if (client.Server.Player.UserAccount.GroupId.HasValue)
            {

                EssentialCommandsGroup group = EssentialCommands.Database.GetGroup(client.Server.Player.UserAccount.GroupId.Value);

                if (group == null)
                    group = EssentialCommands.Database.AddGroup(client.Server.Player.UserAccount.GroupId.Value, null);

                var planets = EssentialCommands.Database.GetUserPlanets(client.Server.Player.UserAccount.Id);

                if (planets.Count >= group.ProtectedPlanetLimit)
                {

                    client.SendChatMessage("Server", "You have reached the limit of planets you can protect!");

                    return;

                }

            }

            planet = EssentialCommands.Database.AddProtectedPlanet(client.Server.Player.Coordinates, client.Server.Player.UserAccount.Id);

            client.SendChatMessage("Server", "Planet now protected!");

            if (planet != null && !_planets.ContainsKey(planet))
                _planets.Add(planet, new List<EssentialCommandsBuilder>());

        }

        [Command("unprotect", "Remove a planet's protection")]
        [CommandPermission("planetprotect")]
        public void Unprotect(StarboundClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "unprotect"))
                return;

            if (client.Server.Player.OnShip || client.Server.Player.Coordinates == null)
            {

                client.SendChatMessage("Server", "You are not on a planet!");

                return;

            }

            var planet = EssentialCommands.Database.GetProtectedPlanet(client.Server.Player.Coordinates);

            if (planet == null)
            {

                client.SendChatMessage("Server", "Planet is not protected!");

                return;

            }

            if (planet.OwnerId != client.Server.Player.UserAccount.Id && !EssentialCommands.IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not own this planet!");

                return;

            }

            _planets.Remove(planet);

            EssentialCommands.Database.RemoveProtectedPlanet(client.Server.Player.Coordinates);

            client.SendChatMessage("Server", "Planet is no longer protected!");

        }

        [Command("addbuilder", "Allows a user to build on the planet")]
        [CommandPermission("planetprotect")]
        public void AddBuilder(StarboundClient client, string[] args)
        {

            if (args.Length != 1)
            {

                client.SendChatMessage("Server", "Syntax: /addbuilder <username>");

                return;

            }

            if (!EssentialCommands.CanUserAccess(client, "addbuilder"))
                return;

            SharpStarUser user = SharpStarMain.Instance.Database.GetUser(args[0]);

            if (user == null)
            {

                client.SendChatMessage("Server", "There is no user by that name!");

                return;

            }

            if (client.Server.Player.Coordinates == null || client.Server.Player.OnShip)
            {

                client.SendChatMessage("Server", "You are not on a planet!");


                return;

            }

            var planet = EssentialCommands.Database.GetProtectedPlanet(client.Server.Player.Coordinates);

            if (planet == null)
            {

                client.SendChatMessage("Server", "Planet is not protected!");

                return;

            }

            if (planet.OwnerId != client.Server.Player.UserAccount.Id && !EssentialCommands.IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not own this planet!");

                return;

            }

            EssentialCommandsBuilder builder = EssentialCommands.Database.GetPlanetBuilder(user.Id, planet.Id);

            if (builder != null)
            {

                client.SendChatMessage("Server", "User is already a builder on this planet!");

                return;

            }

            builder = EssentialCommands.Database.AddPlanetBuilder(planet, user.Id, planet.Id);

            client.SendChatMessage("Server", String.Format("User {0} can now build on this planet", user.Username));

            if (builder != null)
            {
                var x = _planets.SingleOrDefault(p => p.Key == planet);

                if (x.Value.All(p => p.Id != builder.Id))
                    x.Value.Add(builder);
            }

        }

        [Command("removebuilder", "Disallow a user from building on a planet")]
        [CommandPermission("planetprotect")]
        public void RemoveBuilder(StarboundClient client, string[] args)
        {

            if (args.Length != 1)
            {

                client.SendChatMessage("Server", "Syntax: /removebuilder <username>");

                return;

            }

            if (!EssentialCommands.CanUserAccess(client, "removebuilder"))
                return;

            SharpStarUser user = SharpStarMain.Instance.Database.GetUser(args[0]);

            if (user == null)
            {

                client.SendChatMessage("Server", "There is no user by that name!");

                return;

            }

            if (client.Server.Player.Coordinates == null || client.Server.Player.OnShip)
            {

                client.SendChatMessage("Server", "You are not on a planet!");


                return;

            }

            var planet = EssentialCommands.Database.GetProtectedPlanet(client.Server.Player.Coordinates);

            if (planet == null)
            {

                client.SendChatMessage("Server", "Planet is not protected!");

                return;

            }

            if (planet.OwnerId != client.Server.Player.UserAccount.Id && !EssentialCommands.IsAdmin(client))
            {

                client.SendChatMessage("Server", "You do not own this planet!");

                return;

            }

            EssentialCommandsBuilder builder = EssentialCommands.Database.GetPlanetBuilder(user.Id, planet.Id);

            if (builder == null)
            {

                client.SendChatMessage("Server", "The user is not a builder");

                return;

            }

            EssentialCommands.Database.RemovePlanetBuilder(planet, user.Id, planet.Id);

            client.SendChatMessage("Server", String.Format("User {0} can no longer build on this planet", user.Username));


            var x = _planets.SingleOrDefault(p => p.Key == planet);

            if (x.Value != null)
            {
                x.Value.Remove(builder);
            }

        }

        [Event("packetReceived")]
        public void OnTryBuild(IPacket packet, StarboundClient client)
        {

            if (_planets.Count == 0)
                return;

            KnownPacket p = (KnownPacket)packet.PacketId;

            if (p == KnownPacket.ModifyTileList || p == KnownPacket.DamageTileGroup || p == KnownPacket.DamageTile || p == KnownPacket.ConnectWire || p == KnownPacket.DisconnectAllWires
                || p == KnownPacket.EntityCreate)
            {

                if (client.Server.Player.Coordinates != null && !client.Server.Player.OnShip)
                {

                    if (client.Server.Player.UserAccount != null)
                    {

                        if (client.Server.Player.UserAccount.IsAdmin) //all admins are allowed to build
                            return;

                        var planets = _planets.SingleOrDefault(w => w.Key == client.Server.Player.Coordinates);

                        if (planets.Value == null)
                            return;

                        EssentialCommandsBuilder builder = planets.Value.SingleOrDefault(w => w.UserId == client.Server.Player.UserAccount.Id && w.Allowed);

                        if (builder == null && planets.Key.OwnerId != client.Server.Player.UserAccount.Id)
                        {

                            if (p == KnownPacket.EntityCreate)
                            {

                                if (EssentialCommands.Config.ConfigFile.AllowProjectiles)
                                    return;

                                var ec = (EntityCreatePacket)packet;

                                foreach (Entity ent in ec.Entities.Where(x => x.EntityType == EntityType.Projectile))
                                {
                                    if (ent.EntityType == EntityType.Projectile)
                                    {

                                        ProjectileEntity pent = (ProjectileEntity)ent;

                                        if (pent.ThrowerEntityId != client.Server.Player.EntityId)
                                            continue;

                                        if (EssentialCommands.Config.ConfigFile.ProjectileWhitelist.Any(x => x.Equals(pent.Projectile, StringComparison.OrdinalIgnoreCase)))
                                            continue;

                                        pent.Projectile = EssentialCommands.Config.ConfigFile.ReplaceProjectileWith;

                                    }
                                }

                            }
                            else
                            {
                                packet.Ignore = true;
                            }

                        }

                    }
                    else
                    {

                        var planets = _planets.SingleOrDefault(w => w.Key == client.Server.Player.Coordinates);

                        if (planets.Value != null)
                        {

                            if (p == KnownPacket.EntityCreate)
                            {

                                if (EssentialCommands.Config.ConfigFile.AllowProjectiles)
                                    return;

                                var ec = (EntityCreatePacket)packet;

                                foreach (Entity ent in ec.Entities)
                                {
                                    if (ent.EntityType == EntityType.Projectile)
                                    {

                                        ProjectileEntity pent = (ProjectileEntity)ent;

                                        if (pent.ThrowerEntityId != client.Server.Player.EntityId)
                                            continue;

                                        if (EssentialCommands.Config.ConfigFile.ProjectileWhitelist.Any(x => x.Equals(pent.Projectile, StringComparison.OrdinalIgnoreCase)))
                                            continue;

                                        pent.Projectile = EssentialCommands.Config.ConfigFile.ReplaceProjectileWith;

                                    }
                                }

                            }
                            else
                            {
                                packet.Ignore = true;
                            }

                        }

                    }

                }

            }

        }

        private void RefreshProtectedPlanets()
        {

            _planets = new Dictionary<EssentialCommandsPlanet, List<EssentialCommandsBuilder>>();

            var planets = EssentialCommands.Database.GetPlanets();

            foreach (var planet in planets)
            {

                var builders = EssentialCommands.Database.GetPlanetBuilders(planet.Id);

                _planets.Add(planet, builders);

            }

        }

    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly Dictionary<EssentialCommandsPlanet, List<EssentialCommandsBuilder>> _planets;

        public ProtectPlanetCommands()
        {
            _planets = new Dictionary<EssentialCommandsPlanet, List<EssentialCommandsBuilder>>();
            RefreshProtectedPlanets();
        }

        [Command("protect", "Protect a planet")]
        [CommandPermission("planetprotect")]
        public void Protect(SharpStarClient client, string[] args)
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
        public void Unprotect(SharpStarClient client, string[] args)
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
        public void AddBuilder(SharpStarClient client, string[] args)
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
                var x = _planets.First(p => p.Key == planet);

                x.Value.Add(builder);
            }

        }

        [Command("removebuilder", "Disallow a user from building on a planet")]
        [CommandPermission("planetprotect")]
        public void RemoveBuilder(SharpStarClient client, string[] args)
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

            var x = _planets.First(p => p.Key == planet);

            x.Value.RemoveAll(p => p.UserId == builder.UserId);
        }

        [PacketEvent(KnownPacket.ModifyTileList, KnownPacket.DamageTileGroup, KnownPacket.DamageTile, KnownPacket.ConnectWire, KnownPacket.DisconnectAllWires,
            KnownPacket.EntityCreate, KnownPacket.SpawnEntity, KnownPacket.TileLiquidUpdate)]
        public async void OnTryBuild(IPacket packet, SharpStarClient client)
        {

            if (_planets == null || _planets.Count == 0)
                return;

            KnownPacket p = (KnownPacket)packet.PacketId;

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

                            foreach (Entity ent in ec.Entities)
                            {
                                if (ent.EntityType == EntityType.Effect && client.Direction == Direction.Server)
                                {
                                    packet.Ignore = true;

                                    break;
                                }
                                if (ent.EntityType == EntityType.Projectile)
                                {

                                    ProjectileEntity pent = (ProjectileEntity)ent;

                                    if (pent.ThrowerEntityId != client.Server.Player.EntityId)
                                        continue;

                                    if (EssentialCommands.Config.ConfigFile.ProjectileWhitelist.Any(x => x.Equals(pent.Projectile, StringComparison.OrdinalIgnoreCase)))
                                        continue;

                                    if (string.IsNullOrEmpty(EssentialCommands.Config.ConfigFile.ReplaceProjectileWith))
                                    {
                                        await client.Server.PlayerClient.SendPacket(new EntityDestroyPacket
                                        {
                                            EntityId = ent.EntityId,
                                            Unknown = new byte[0]
                                        });

                                        packet.Ignore = true;

                                        break;
                                    }

                                    pent.Projectile = EssentialCommands.Config.ConfigFile.ReplaceProjectileWith;

                                }
                            }

                        }
                        else if (p == KnownPacket.SpawnEntity)
                        {
                            if (EssentialCommands.Config.ConfigFile.AllowProjectiles)
                                return;

                            var ec = (SpawnEntityPacket)packet;

                            foreach (SpawnedEntity ent in ec.SpawnedEntities)
                            {
                                if (ent.EntityType == EntityType.Projectile && ent is SpawnedProjectile)
                                {
                                    SpawnedProjectile sp = (SpawnedProjectile)ent;

                                    if (!EssentialCommands.Config.ConfigFile.ProjectileWhitelist.Any(x => x.Equals(sp.ProjectileKey, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        packet.Ignore = true;

                                        break;
                                    }
                                }
                                else if (ent.EntityType == EntityType.Object || ent.EntityType == EntityType.Plant || ent.EntityType == EntityType.PlantDrop || ent.EntityType == EntityType.Monster
                                    || ent.EntityType == EntityType.Effect)
                                {
                                    packet.Ignore = true;

                                    break;
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
                                if (ent.EntityType == EntityType.Effect && client.Direction == Direction.Server)
                                {
                                    packet.Ignore = true;

                                    break;
                                }
                                if (ent.EntityType == EntityType.Projectile)
                                {

                                    ProjectileEntity pent = (ProjectileEntity)ent;

                                    if (pent.ThrowerEntityId != client.Server.Player.EntityId)
                                        continue;

                                    if (EssentialCommands.Config.ConfigFile.ProjectileWhitelist.Any(x => x.Equals(pent.Projectile, StringComparison.OrdinalIgnoreCase)))
                                        continue;

                                    if (string.IsNullOrEmpty(EssentialCommands.Config.ConfigFile.ReplaceProjectileWith))
                                    {
                                        await client.Server.PlayerClient.SendPacket(new EntityDestroyPacket
                                        {
                                            EntityId = ent.EntityId,
                                            Unknown = new byte[0]
                                        });

                                        packet.Ignore = true;

                                        break;
                                    }

                                    pent.Projectile = EssentialCommands.Config.ConfigFile.ReplaceProjectileWith;

                                }
                            }
                        }
                        else if (p == KnownPacket.SpawnEntity)
                        {
                            if (EssentialCommands.Config.ConfigFile.AllowProjectiles)
                                return;

                            var ec = (SpawnEntityPacket)packet;

                            foreach (SpawnedEntity ent in ec.SpawnedEntities)
                            {
                                if (ent.EntityType == EntityType.Projectile && ent is SpawnedProjectile)
                                {
                                    SpawnedProjectile sp = (SpawnedProjectile)ent;

                                    if (!EssentialCommands.Config.ConfigFile.ProjectileWhitelist.Any(x => x.Equals(sp.ProjectileKey, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        packet.Ignore = true;

                                        break;
                                    }
                                }
                                else if (ent.EntityType == EntityType.Object || ent.EntityType == EntityType.Plant || ent.EntityType == EntityType.PlantDrop || ent.EntityType == EntityType.Monster
                                    || ent.EntityType == EntityType.Effect)
                                {
                                    packet.Ignore = true;

                                    break;
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

        private void RefreshProtectedPlanets()
        {
            _planets.Clear();

            var planets = EssentialCommands.Database.GetPlanets();

            foreach (var planet in planets)
            {
                var builders = EssentialCommands.Database.GetPlanetBuilders(planet.Id);

                _planets.Add(planet, builders);

            }
        }

    }
}

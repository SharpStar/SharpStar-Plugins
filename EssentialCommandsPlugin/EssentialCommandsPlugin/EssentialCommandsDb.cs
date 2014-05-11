using System;
using System.Collections.Generic;
using System.Linq;
using SharpStar.Lib.Database;
using SharpStar.Lib.DataTypes;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsDb : ISharpStarDb
    {

        public string DatabaseName { get; private set; }

        public EssentialCommandsDb(string dbName)
        {

            DatabaseName = dbName;

            CreateTables();

        }

        public void CreateTables()
        {

            var conn = new SQLiteConnection(DatabaseName);

            conn.CreateTable<EssentialCommandsBan>();
            conn.CreateTable<EssentialCommandsShip>();
            conn.CreateTable<EssentialCommandsShipUser>();
            conn.CreateTable<EssentialCommandsMute>();
            conn.CreateTable<EssentialCommandsPlanet>();
            conn.CreateTable<EssentialCommandsBuilder>();
            conn.CreateTable<EssentialCommandsGroup>();

            conn.Close();
            conn.Dispose();

        }

        public EssentialCommandsShipUser AddShipUser(int userId, int shipId, bool allowed = true)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var ship = conn.Get<EssentialCommandsShip>(shipId);

            if (ship == null || GetShipUser(userId, shipId) != null)
            {

                conn.Close();
                conn.Dispose();

                return null;

            }

            var usr = new EssentialCommandsShipUser
            {
                UserAccountId = userId,
                HasAccess = allowed,
                ShipId = shipId
            };

            conn.Insert(usr);

            conn.Close();
            conn.Dispose();

            return usr;

        }

        public EssentialCommandsShipUser GetShipUser(int userId, int shipId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsShipUser>();

            var usr = tbl.SingleOrDefault(p => p.UserAccountId == userId && p.ShipId == shipId);

            conn.Close();
            conn.Dispose();

            return usr;

        }

        public List<EssentialCommandsShipUser> GetShipUsers(int id)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var ship = conn.Get<EssentialCommandsShip>(id);

            if (ship == null)
                return new List<EssentialCommandsShipUser>();

            var users = conn.Table<EssentialCommandsShipUser>().Where(p => p.Id == id);

            var usrList = users.ToList();

            conn.Close();
            conn.Dispose();

            return usrList;

        }

        public void UpdateShipUser(EssentialCommandsShipUser user)
        {

            var conn = new SQLiteConnection(DatabaseName);

            conn.Update(user);

            conn.Close();
            conn.Dispose();

        }

        public void RemoveShipUser(int userId, int shipId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var usr = GetShipUser(userId, shipId);

            if (usr == null)
            {

                conn.Close();
                conn.Dispose();

                return;

            }

            conn.Delete<EssentialCommandsShipUser>(usr.Id);

        }

        public EssentialCommandsShip GetShip(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var ship = conn.Table<EssentialCommandsShip>().SingleOrDefault(p => p.OwnerUserAccountId == userId);

            conn.Close();
            conn.Dispose();

            return ship;

        }

        public EssentialCommandsShip AddShip(int userId, bool isPublic = true)
        {

            if (GetShip(userId) != null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            var ship = new EssentialCommandsShip
            {
                OwnerUserAccountId = userId,
                Public = isPublic
            };

            conn.Insert(ship);

            conn.Close();
            conn.Dispose();

            return ship;

        }

        public void UpdateShip(EssentialCommandsShip ship)
        {

            var conn = new SQLiteConnection(DatabaseName);

            conn.Update(ship);

            conn.Close();
            conn.Dispose();

        }

        public void RemoveShip(int shipId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            conn.Delete<EssentialCommandsShip>(shipId);

            var users = GetShipUsers(shipId);

            foreach (var usr in users)
            {
                conn.Delete(usr);
            }

        }

        public List<EssentialCommandsBan> GetBans()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            var bans = tbl.ToList();

            conn.Close();
            conn.Dispose();

            return bans;

        }

        public void AddBan(string uuid, int? userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            var b = tbl.SingleOrDefault(p => p.UUID == uuid || (p.UserAccountId.HasValue && userId.HasValue && userId.Value == p.UserAccountId.Value));

            if (b == null)
            {

                EssentialCommandsBan ban = new EssentialCommandsBan();
                ban.UUID = uuid;

                if (userId.HasValue)
                    ban.UserAccountId = userId.Value;

                conn.Insert(ban);

            }
            else
            {

                b.UUID = uuid;

                if (userId.HasValue)
                    b.UserAccountId = userId;

                conn.Update(b);

            }

            conn.Close();
            conn.Dispose();

        }

        public void RemoveBan(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            var user = tbl.SingleOrDefault(p => p.UserAccountId == userId);

            if (user == null)
                return;

            conn.Delete(user);

            conn.Close();
            conn.Dispose();

        }

        public bool AddMute(int userId, DateTime? expireTime)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsMute>();

            if (tbl.Any(p => p.UserId == userId))
                return false;

            var mute = new EssentialCommandsMute
            {
                UserId = userId,
                ExpireTime = expireTime
            };

            conn.Insert(mute);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public bool RemoveMute(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsMute>();

            EssentialCommandsMute mute = tbl.SingleOrDefault(p => p.UserId == userId);

            if (mute == null)
                return false;

            conn.Delete<EssentialCommandsMute>(mute.Id);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public bool IsUserMuted(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsMute>();

            EssentialCommandsMute mute = tbl.SingleOrDefault(p => p.UserId == userId);

            conn.Close();
            conn.Dispose();

            return mute != null;

        }

        public List<EssentialCommandsMute> GetMutedUsers()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsMute>();

            var mutedUsers = tbl.ToList();

            conn.Close();
            conn.Dispose();

            return mutedUsers;

        }

        public List<EssentialCommandsPlanet> GetPlanets()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsPlanet>();

            var planets = tbl.ToList();

            conn.Close();
            conn.Dispose();

            return planets;

        }

        public EssentialCommandsPlanet GetProtectedPlanet(WorldCoordinate coords)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsPlanet>();

            var planet = tbl.SingleOrDefault(p => p == coords);

            conn.Close();
            conn.Dispose();

            return planet;

        }

        public EssentialCommandsPlanet AddProtectedPlanet(WorldCoordinate coords, int ownerId)
        {

            if (GetProtectedPlanet(coords) != null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            EssentialCommandsPlanet planet = new EssentialCommandsPlanet(coords);
            planet.OwnerId = ownerId;

            planet.Id = conn.Insert(planet);

            conn.Close();
            conn.Dispose();

            return planet;
        
        }

        public bool RemoveProtectedPlanet(WorldCoordinate coords)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsPlanet>();

            var planet = tbl.SingleOrDefault(p => p == coords);

            if (planet == null)
                return false;

            var builders = GetPlanetBuilders(planet.Id);

            foreach (var builder in builders) //delete all builders associated with the planet
            {
                conn.Delete<EssentialCommandsBuilder>(builder.Id);
            }

            conn.Delete<EssentialCommandsPlanet>(planet.Id);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public EssentialCommandsBuilder AddPlanetBuilder(WorldCoordinate coords, int userId, int planetId, bool allowed = true)
        {

            EssentialCommandsPlanet planet = GetProtectedPlanet(coords);

            if (planet == null)
                return null;

            if (GetPlanetBuilder(userId, planetId) != null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            var builder = new EssentialCommandsBuilder
            {
                UserId = userId,
                PlanetId = planetId,
                Allowed = allowed
            };

            int id = conn.Insert(builder);

            builder.Id = id;

            conn.Close();
            conn.Dispose();

            return builder;

        }

        public bool RemovePlanetBuilder(WorldCoordinate coords, int userId, int planetId)
        {

            EssentialCommandsPlanet planet = GetProtectedPlanet(coords);

            if (planet == null)
                return false;

            var builder = GetPlanetBuilder(userId, planetId);

            if (builder == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            conn.Delete<EssentialCommandsBuilder>(builder.Id);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public EssentialCommandsBuilder GetPlanetBuilder(int userId, int planetId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBuilder>();

            var builder = tbl.SingleOrDefault(p => p.UserId == userId && p.PlanetId == planetId);

            conn.Close();
            conn.Dispose();

            return builder;

        }

        public List<EssentialCommandsBuilder> GetPlanetBuilders(int planetId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBuilder>();

            var builders = tbl.Where(p => p.PlanetId == planetId).ToList();

            conn.Close();
            conn.Dispose();

            return builders;

        }

        public EssentialCommandsGroup AddGroup(int groupId, string prefix)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsGroup>();

            EssentialCommandsGroup group = tbl.SingleOrDefault(p => p.GroupId == groupId);

            if (group != null)
            {

                conn.Close();
                conn.Dispose();

                return null;

            }

            group = new EssentialCommandsGroup
            {
                GroupId = groupId,
                Prefix = prefix
            };

            group.Id = conn.Insert(group);

            conn.Close();
            conn.Dispose();

            return group;

        }

        public bool RemoveGroup(int groupId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsGroup>();

            EssentialCommandsGroup group = tbl.SingleOrDefault(p => p.GroupId == groupId);

            if (group == null)
            {

                conn.Close();
                conn.Dispose();

                return false;

            }

            conn.Delete<SharpStarGroupPermission>(group.Id);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public bool SetGroupPrefix(int groupId, string prefix)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsGroup>();

            EssentialCommandsGroup group = tbl.SingleOrDefault(p => p.GroupId == groupId);

            if (group == null)
            {

                conn.Close();
                conn.Dispose();

                return false;

            }

            group.Prefix = prefix;

            conn.Update(group);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public EssentialCommandsGroup GetGroup(int groupId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsGroup>();

            EssentialCommandsGroup group = tbl.SingleOrDefault(p => p.GroupId == groupId);

            conn.Close();
            conn.Dispose();

            return group;

        }

        public List<EssentialCommandsGroup> GetGroups()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsGroup>();

            var groups = tbl.ToList();

            conn.Close();
            conn.Dispose();

            return groups;

        }

    }
}

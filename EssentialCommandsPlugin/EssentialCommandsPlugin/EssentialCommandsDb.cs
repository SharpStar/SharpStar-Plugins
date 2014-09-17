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
            conn.CreateTable<EssentialCommandsBanUUID>();
            conn.CreateTable<EssentialCommandsShip>();
            conn.CreateTable<EssentialCommandsShipUser>();
            conn.CreateTable<EssentialCommandsMute>();
            conn.CreateTable<EssentialCommandsPlanet>();
            conn.CreateTable<EssentialCommandsBuilder>();
            conn.CreateTable<EssentialCommandsGroup>();
            conn.CreateTable<EssentialCommandsCommand>();
            conn.CreateTable<EssentialCommandsUserCommand>();

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

        public EssentialCommandsBan GetBanByUserId(int userId)
        {
            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            var ban = tbl.FirstOrDefault(p => p.UserAccountId == userId);

            conn.Close();
            conn.Dispose();

            return ban;
        }

        public EssentialCommandsBan GetBan(int id)
        {
            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            EssentialCommandsBan ban = tbl.SingleOrDefault(p => p.Id == id);

            conn.Close();
            conn.Dispose();

            return ban;
        }

        public List<EssentialCommandsBanUUID> GetBansUuid(int banId)
        {
            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBanUUID>();

            var bans = tbl.Where(p => p.BanId == banId).ToList();

            conn.Close();
            conn.Dispose();

            return bans;
        }

        public EssentialCommandsBanUUID GetBansUuid(string uuid)
        {
            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBanUUID>();

            var ban = tbl.FirstOrDefault(p => p.UUID == uuid);

            conn.Close();
            conn.Dispose();

            return ban;
        }

        public void AddBan(string uuid, string playerName, string reason, DateTime? expireTime, int? userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            var b = tbl.SingleOrDefault(p => p.UserAccountId.HasValue && userId.HasValue && userId.Value == p.UserAccountId.Value);

            int banId;
            if (b == null)
            {
                EssentialCommandsBan ban = new EssentialCommandsBan();
                ban.BanReason = reason;
                ban.ExpirationTime = expireTime;

                if (userId.HasValue)
                    ban.UserAccountId = userId.Value;

                conn.Insert(ban);

                banId = ban.Id;
            }
            else
            {
                banId = b.Id;

                b.BanReason = reason;
                b.ExpirationTime = expireTime;

                if (userId.HasValue)
                    b.UserAccountId = userId;

                conn.Update(b);
            }

            if (conn.Table<EssentialCommandsBanUUID>().All(p => p.UUID != uuid))
            {
                EssentialCommandsBanUUID ecUUid = new EssentialCommandsBanUUID { UUID = uuid, BanId = banId, PlayerName = playerName };
                conn.InsertOrReplace(ecUUid);
            }

            conn.Close();
            conn.Dispose();

        }

        public void RemoveBanByUserId(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBan>();

            var ban = tbl.SingleOrDefault(p => p.UserAccountId == userId);

            if (ban == null)
                return;

            var tbl2 = conn.Table<EssentialCommandsBanUUID>();

            var uuidBans = tbl2.Where(p => p.BanId == ban.Id);

            foreach (EssentialCommandsBanUUID uuidBan in uuidBans)
            {
                conn.Delete<EssentialCommandsBanUUID>(uuidBan.Id);
            }

            conn.Delete<EssentialCommandsBan>(ban.Id);

            conn.Close();
            conn.Dispose();

        }

        public void RemoveBan(int id)
        {
            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBanUUID>();

            var uuidBans = tbl.Where(p => p.BanId == id);

            foreach (EssentialCommandsBanUUID uuidBan in uuidBans)
            {
                conn.Delete<EssentialCommandsBanUUID>(uuidBan.Id);
            }

            conn.Delete<EssentialCommandsBan>(id);

            conn.Close();
            conn.Dispose();

        }

        public void RemoveBanUuid(string uuid)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsBanUUID>();

            var ban = tbl.FirstOrDefault(p => p.UUID == uuid);

            if (ban == null)
                return;

            conn.Delete(ban);

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

        public List<EssentialCommandsPlanet> GetUserPlanets(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsPlanet>();

            var planets = tbl.Where(p => p.OwnerId == userId).ToList();

            conn.Close();
            conn.Dispose();

            return planets;

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

        public bool SetGroupPlanetLimit(int groupId, int? limit)
        {

            var group = GetGroup(groupId);

            if (group == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            group.ProtectedPlanetLimit = limit;

            conn.Update(group);

            conn.Close();
            conn.Dispose();

            return true;

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

        public EssentialCommandsCommand AddCommand(int groupId, string command, int limit)
        {

            if (GetCommand(groupId, command) != null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            var cmd = new EssentialCommandsCommand
            {
                GroupId = groupId,
                Command = command,
                Limit = limit
            };

            cmd.Id = conn.Insert(cmd);

            conn.Close();
            conn.Dispose();

            return cmd;

        }

        public bool RemoveCommand(int groupId, string command)
        {

            EssentialCommandsCommand cmd = GetCommand(groupId, command);

            if (cmd == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            conn.Delete<EssentialCommandsCommand>(cmd.Id);

            var usrTbl = conn.Table<EssentialCommandsUserCommand>();

            var usrCmds = usrTbl.Where(p => p.CommandId == cmd.Id);

            foreach (EssentialCommandsUserCommand usrCmd in usrCmds)
            {
                conn.Delete(usrCmd);
            }

            conn.Close();
            conn.Dispose();

            return true;

        }

        public List<EssentialCommandsCommand> GetCommands(int groupId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsCommand>();

            var cmds = tbl.Where(p => p.GroupId == groupId).ToList();

            conn.Close();
            conn.Dispose();

            return cmds;

        }

        public EssentialCommandsCommand GetCommand(int groupId, string command)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsCommand>();

            EssentialCommandsCommand cmd = tbl.SingleOrDefault(p => p.GroupId == groupId && p.Command == command);

            conn.Close();
            conn.Dispose();

            return cmd;

        }

        public bool SetCommandLimit(int groupId, string command, int? limit)
        {

            EssentialCommandsCommand cmd = GetCommand(groupId, command);

            if (cmd == null)
                return false;

            cmd.Limit = limit;

            var conn = new SQLiteConnection(DatabaseName);

            conn.Update(cmd);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public void RemoveUserCommmands(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsUserCommand>();

            var usrCmds = tbl.Where(p => p.UserId == userId);

            foreach (var usrCmd in usrCmds)
            {
                conn.Delete(usrCmd);
            }

            conn.Close();
            conn.Dispose();

        }

        public bool IncCommandTimesUsed(int userId, int groupId, string command)
        {

            var grpCmd = GetCommand(groupId, command);

            if (grpCmd == null)
                return false;

            var cmd = GetUserCommand(userId, groupId, command);

            var conn = new SQLiteConnection(DatabaseName);

            if (cmd == null)
            {
                conn.Insert(new EssentialCommandsUserCommand
                {
                    UserId = userId,
                    CommandId = grpCmd.Id,
                    TimesUsed = 1
                });
            }
            else
            {
                cmd.TimesUsed += 1;
                conn.Update(cmd);
            }

            conn.Close();
            conn.Dispose();

            return true;

        }

        public EssentialCommandsUserCommand GetUserCommand(int userId, int groupId, string command)
        {

            EssentialCommandsCommand cmd = GetCommand(groupId, command);

            if (cmd == null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<EssentialCommandsUserCommand>();

            EssentialCommandsUserCommand usrCmd = tbl.SingleOrDefault(p => p.UserId == userId && p.CommandId == cmd.Id);

            conn.Close();
            conn.Dispose();

            return usrCmd;

        }

    }
}

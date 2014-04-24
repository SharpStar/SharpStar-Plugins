using System;
using System.Collections.Generic;
using System.Linq;
using SharpStar.Lib.Database;
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

    }
}

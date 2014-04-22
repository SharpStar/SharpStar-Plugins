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

            conn.Close();
            conn.Dispose();

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

    }
}

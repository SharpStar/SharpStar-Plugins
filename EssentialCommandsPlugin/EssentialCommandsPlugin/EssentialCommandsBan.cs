using System;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsBan
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string BanReason { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public int? UserAccountId { get; set; }

    }
}

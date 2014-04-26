using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsMute
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime? ExpireTime { get; set; }

    }
}

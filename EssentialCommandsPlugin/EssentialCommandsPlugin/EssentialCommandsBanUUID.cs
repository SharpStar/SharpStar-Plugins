using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsBanUUID
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int BanId { get; set; }

        public string PlayerName { get; set; }

        public string UUID { get; set; }

    }
}

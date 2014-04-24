using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsShip
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int OwnerUserAccountId { get; set; }

        public bool Public { get; set; }

    }
}

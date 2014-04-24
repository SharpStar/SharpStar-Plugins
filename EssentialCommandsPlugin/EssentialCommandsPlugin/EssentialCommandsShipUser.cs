using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsShipUser
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserAccountId { get; set; }

        public int ShipId { get; set; }

        public bool HasAccess { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsBuilder
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int PlanetId { get; set; }

        public bool Allowed { get; set; }

    }
}

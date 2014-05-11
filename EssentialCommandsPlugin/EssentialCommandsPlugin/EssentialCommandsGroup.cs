using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsGroup
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int GroupId { get; set; }

        public string Prefix { get; set; }

    }
}

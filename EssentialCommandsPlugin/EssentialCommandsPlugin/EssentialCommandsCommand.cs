using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsCommand
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int GroupId { get; set; }

        public string Command { get; set; }

        public int? Limit { get; set; }

    }
}

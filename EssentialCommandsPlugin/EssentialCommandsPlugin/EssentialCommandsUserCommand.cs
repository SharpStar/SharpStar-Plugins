using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsUserCommand
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CommandId { get; set; }

        public int TimesUsed { get; set; }

    }
}

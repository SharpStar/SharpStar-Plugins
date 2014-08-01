using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPIPlugin
{
    public class APISharpStarAccount
    {

        public int ID { get; set; }

        public string Username { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsOnline { get; set; }

        public int? GroupId { get; set; }

        public DateTime? LastLogin { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpStarWebPlugin.Responses
{
    public class SharpStarPlayer : SharpStarResponse
    {

        public string Name { get; set; }

        public string AuthenticatedUsername { get; set; }

        public bool IsAdmin { get; set; }

        public int Duration { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpStarWebPlugin.Responses
{

    public class StatusResponse
    {

        public StatusResponseType Type { get; set; }

        public object Value { get; set; }

    }

    public enum StatusResponseType
    {
        Connect = 1,
        Disconnect = 2,
        ChatMessage = 3,
        Authenticated = 4
    }

}

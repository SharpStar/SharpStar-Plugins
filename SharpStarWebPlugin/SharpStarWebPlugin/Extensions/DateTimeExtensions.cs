using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpStarWebPlugin.Extensions
{
    public static class DateTimeExtensions
    {

        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static DateTime ToDateTime(this long timestamp)
        {
            return _epoch.AddSeconds(timestamp).ToLocalTime();
        }

        public static long ToUnixTimeStamp(this DateTime time)
        {

            var timeSpan = (time.ToUniversalTime() - _epoch);

            return (long)timeSpan.TotalSeconds;
        
        }

    }
}

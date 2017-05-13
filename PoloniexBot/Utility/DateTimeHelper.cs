using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility {
    static class DateTimeHelper {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long clockOffset;

        public static void RecalculateClockOffset () {
            TimeSpan offset = DateTime.Now.Subtract(DateTime.Now.ToUniversalTime());
            clockOffset = (long)offset.TotalSeconds;
        }

        public static long DateTimeToUnixTimestamp (DateTime value) {
            return (long)(value - Epoch).TotalSeconds;
        }
        public static DateTime UnixTimestampToDateTime (long timestamp) {
            return Epoch.AddSeconds(timestamp);
        }

        public static long GetServerTime (long timestamp) {
            return timestamp - clockOffset;
        }
        public static long GetClientTime (long timestamp) {
            return timestamp + clockOffset;
        }
    }
}

using Newtonsoft.Json;
using System;

namespace PoloniexAPI.TradingTools {
    public class Trade : Order, ITrade {
        [JsonProperty("date")]
        private string TimeInternal {
            set { Time = Helper.ParseDateTime(value); }
        }
        public DateTime Time { get; private set; }
    }
}

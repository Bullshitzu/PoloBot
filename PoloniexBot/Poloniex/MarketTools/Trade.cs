using Newtonsoft.Json;
using System;

namespace PoloniexAPI.MarketTools {
    public class Trade : ITrade, IComparable<Trade> {
        [JsonProperty("date")]
        private string TimeInternal {
            set { Time = Helper.ParseDateTime(value); }
        }
        public DateTime Time { get; private set; }

        [JsonProperty("type")]
        private string TypeInternal {
            set { Type = value.ToOrderType(); }
        }
        public OrderType Type { get; private set; }

        [JsonProperty("rate")]
        public double PricePerCoin { get; private set; }

        [JsonProperty("amount")]
        public double AmountQuote { get; private set; }
        [JsonProperty("total")]
        public double AmountBase { get; private set; }

        public Trade (string date, string type, string rate, string amount, string total) {
            TimeInternal = date;
            TypeInternal = type;
            PricePerCoin = double.Parse(rate, System.Globalization.CultureInfo.InvariantCulture);
            AmountQuote = double.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);
            AmountBase = double.Parse(total, System.Globalization.CultureInfo.InvariantCulture);
        }

        public int CompareTo (Trade other) {
            return this.Time.CompareTo(other.Time);
        }
    }
}

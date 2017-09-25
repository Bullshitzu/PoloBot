using Newtonsoft.Json;

namespace PoloniexAPI.MarketTools {
    public class MarketData : IMarketData {
        [JsonProperty("last")]
        public double PriceLast { get; internal set; }
        [JsonProperty("percentChange")]
        public double PriceChangePercentage { get; internal set; }

        [JsonProperty("baseVolume")]
        public double Volume24HourBase { get; internal set; }
        [JsonProperty("quoteVolume")]
        public double Volume24HourQuote { get; internal set; }

        [JsonProperty("highestBid")]
        public double OrderTopBuy { get; internal set; }
        [JsonProperty("lowestAsk")]
        public double OrderTopSell { get; internal set; }
        public double OrderSpread {
            get { return (OrderTopSell - OrderTopBuy).Normalize(); }
        }
        public double OrderSpreadPercentage {
            get { return OrderTopSell / OrderTopBuy - 1; }
        }

        [JsonProperty("isFrozen")]
        internal byte IsFrozenInternal {
            set { IsFrozen = value != 0; }
        }
        public bool IsFrozen { get; private set; }

        public MarketData (double price) {
            this.PriceLast = price;
            this.OrderTopBuy = price;
            this.OrderTopSell = price;
        }

        public MarketData () { }

        public static bool Equal (IMarketData a, IMarketData b) {

            if (a.PriceLast != b.PriceLast) return false;

            if (a.OrderTopBuy != b.OrderTopBuy) return false;
            if (a.OrderTopSell != b.OrderTopSell) return false;

            if (a.PriceChangePercentage != b.PriceChangePercentage) return false;
            if (a.Volume24HourBase != b.Volume24HourBase) return false;

            return true;
        }
    }
}

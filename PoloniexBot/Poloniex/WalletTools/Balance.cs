using Newtonsoft.Json;

namespace PoloniexAPI.WalletTools {
    public class Balance : IBalance {
        [JsonProperty("available")]
        public double QuoteAvailable { get; private set; }
        [JsonProperty("onOrders")]
        public double QuoteOnOrders { get; private set; }
        [JsonProperty("btcValue")]
        public double BitcoinValue { get; set; }

        public Balance (double availabe, double orders, double btcValue) {
            this.QuoteAvailable = availabe;
            this.QuoteOnOrders = orders;
            this.BitcoinValue = btcValue;
        }
    }
}

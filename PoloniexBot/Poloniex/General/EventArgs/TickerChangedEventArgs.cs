using PoloniexAPI.MarketTools;
using System;

namespace PoloniexAPI {
    public class TickerChangedEventArgs : EventArgs, IComparable<TickerChangedEventArgs>, IDisposable {
        public CurrencyPair CurrencyPair { get; private set; }
        public MarketData MarketData { get; private set; }

        public long Timestamp { get; set; }
        public double ChangeLast { get; set; }

        internal TickerChangedEventArgs (CurrencyPair currencyPair, MarketData marketData) {
            CurrencyPair = currencyPair;
            MarketData = marketData;
        }

        public TickerChangedEventArgs (TickerChangedEventArgs ticker, double newPrice) {
            this.CurrencyPair = ticker.CurrencyPair;
            this.Timestamp = ticker.Timestamp;
            this.ChangeLast = ticker.ChangeLast;
            this.MarketData = new MarketData(newPrice);
        }

        public int CompareTo (TickerChangedEventArgs other) {
            return this.Timestamp.CompareTo(other.Timestamp);
        }

        public void Dispose () {
            this.CurrencyPair = null;
            this.MarketData = null;
        }
    }
}

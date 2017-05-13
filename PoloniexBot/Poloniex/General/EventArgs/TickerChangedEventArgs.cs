using PoloniexAPI.MarketTools;
using System;

namespace PoloniexAPI {
    public class TickerChangedEventArgs : EventArgs, IComparable<TickerChangedEventArgs> {
        public CurrencyPair CurrencyPair { get; private set; }
        public MarketData MarketData { get; private set; }

        public long Timestamp { get; set; }
        public double ChangeLast { get; set; }

        internal TickerChangedEventArgs (CurrencyPair currencyPair, MarketData marketData) {
            CurrencyPair = currencyPair;
            MarketData = marketData;
        }

        public int CompareTo (TickerChangedEventArgs other) {
            return this.Timestamp.CompareTo(other.Timestamp);
        }
    }
}

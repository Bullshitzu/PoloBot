using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility {
    public class MarketDataComparerVolume : IComparer<KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> {
        public int Compare (KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData> x, KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData> y) {
            return x.Value.Volume24HourBase.CompareTo(y.Value.Volume24HourBase);
        }
    }
    public class MarketDataComparerPrice : IComparer<KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> {
        public int Compare (KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData> x, KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData> y) {
            return x.Value.PriceLast.CompareTo(y.Value.PriceLast);
        }
    }
}
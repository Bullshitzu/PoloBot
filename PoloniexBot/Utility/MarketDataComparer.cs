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
    public class MarketDataComparerChange24 : IComparer<KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> {
        public int Compare (KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData> x, KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData> y) {
            return x.Value.PriceChangePercentage.CompareTo(y.Value.PriceChangePercentage);
        }
    }

    public class MarketDataComparerTrend : IComparer<KeyValuePair<PoloniexAPI.CurrencyPair, double>> {
        public int Compare (KeyValuePair<PoloniexAPI.CurrencyPair, double> x, KeyValuePair<PoloniexAPI.CurrencyPair, double> y) {
            return x.Value.CompareTo(y.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class PriceDelta : Predictor {

        public PriceDelta (CurrencyPair pair) : base(pair) { }
        public override void SignResult (ResultSet rs) {
            rs.signature = "Price Delta";
        }

        static int[] IndexPeriods = { 150 }; // seconds
        // coresponds to roughly 10 min / 30 min on the most active pairs

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            rs.variables.Add("price", new ResultSet.Variable("Price", tickers[tickers.Length - 1].MarketData.PriceLast, 8));

            for (int i = 0; i < IndexPeriods.Length; i++) {
                double delta = GetPriceDelta(tickers, IndexPeriods[i]);
                rs.variables.Add("delta" + i, new ResultSet.Variable("Delta " + i, delta, 2));
            }

            SaveResult(rs);
        }

        private double GetPriceDelta (TickerChangedEventArgs[] tickers, int timeDelta) {
            if (tickers == null || tickers.Length == 0) return 0;

            double endPrice = tickers[tickers.Length - 1].MarketData.PriceLast;
            double startPrice = endPrice;

            long startTime = tickers[tickers.Length - 1].Timestamp - timeDelta;
            int startIndex = tickers.Length - 1;

            for (int i = tickers.Length-1; i > 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                startIndex = i;
            }

            startPrice = tickers[startIndex].MarketData.PriceLast;

            return ((endPrice - startPrice) / startPrice) * 100;
        }
    }
}

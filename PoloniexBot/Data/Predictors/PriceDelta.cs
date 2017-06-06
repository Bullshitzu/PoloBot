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

        static int[] IndexPeriods = { 60, 300, 500}; // 3, 5 minutes
        // these will compound to 8 minutes

        // |---------------|----------|-----|
        //      10min             5min      3min

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            rs.variables.Add("priceBuy", new ResultSet.Variable("Buy", tickers.Last().MarketData.OrderTopBuy, 8));
            rs.variables.Add("priceSell", new ResultSet.Variable("Sell", tickers.Last().MarketData.OrderTopSell, 8));

            // calculate price deltas for index periods

            int endIndex = tickers.Length - 1;
            int totalTime = 0;

            double deltaAvg = 0;

            for (int i = 0; i < IndexPeriods.Length; i++) {
                totalTime += IndexPeriods[i];
                double delta = GetPriceDelta(tickers, totalTime, endIndex, ref endIndex);
                deltaAvg += delta / (i + 1);

                rs.variables.Add("delta" + i, new ResultSet.Variable("Delta " + i, delta, 8));
            }

            deltaAvg /= IndexPeriods.Length;
            rs.variables.Add("deltaAvg", new ResultSet.Variable("Delta Avg.", deltaAvg, 8));

            SaveResult(rs);
        }

        private double GetPriceDelta (TickerChangedEventArgs[] tickers, int timeDelta, int startIndex, ref int endIndex) {
            // returns price change in % from startIndex (last) to endIndex (first)
            // (its going in reverse...)

            double startPrice = tickers[startIndex].MarketData.OrderTopBuy;
            double endPrice = startPrice;

            long startTime = tickers[startIndex].Timestamp;
            long endTime = startTime - timeDelta;

            for (int i = startIndex; i >= 0; i--) {
                if (tickers[i].Timestamp < endTime) {
                    endIndex = i;
                    break;
                }
                endPrice = tickers[i].MarketData.OrderTopBuy;
            }

            double delta = ((startPrice - endPrice) / endPrice) * 100;
            return delta;
        }
    }
}

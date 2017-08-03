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

        public static long Timeframe1 = 60; // 1 min
        public static long Timeframe2 = 300; // 5 min
        public static long Timeframe3 = 900; // 30 min

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            rs.variables.Add("priceBuy", new ResultSet.Variable("Buy", tickers.Last().MarketData.OrderTopBuy, 8));
            rs.variables.Add("priceSell", new ResultSet.Variable("Sell", tickers.Last().MarketData.OrderTopSell, 8));

            // calculate price deltas for index periods

            double delta1 = GetPriceDelta(tickers, Timeframe1);
            double delta2 = GetPriceDelta(tickers, Timeframe2);
            double delta3 = GetPriceDelta(tickers, Timeframe3);

            rs.variables.Add("priceDelta1", new ResultSet.Variable("Price Delta 1", delta1, 8));
            rs.variables.Add("priceDelta2", new ResultSet.Variable("Price Delta 2", delta2, 8));
            rs.variables.Add("priceDelta3", new ResultSet.Variable("Price Delta 3", delta3, 8));

            SaveResult(rs);
        }

        private double GetPriceDelta (TickerChangedEventArgs[] tickers, long timeframe) {

            double endPrice = tickers.Last().MarketData.OrderTopBuy;
            double startPrice = endPrice;

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - timeframe;
            
            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                startPrice = tickers[i].MarketData.OrderTopBuy;
            }

            double delta = ((endPrice - startPrice) / startPrice) * 100;
            return delta;
        }
    }
}

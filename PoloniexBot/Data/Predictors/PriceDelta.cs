using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class PriceDelta : Predictor {

        public PriceDelta (CurrencyPair pair, int timeframe = 3600) : base(pair) {
            Timeframe = timeframe;
        }
        public override void SignResult (ResultSet rs) {
            rs.signature = "Price Delta";
        }

        private int Timeframe = 3600;
        private int StartTime = 0;

        public void SetStartTime (int val) {
            this.StartTime = val;
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            rs.variables.Add("priceBuy", new ResultSet.Variable("Buy", tickers.Last().MarketData.OrderTopBuy, 8));
            rs.variables.Add("priceSell", new ResultSet.Variable("Sell", tickers.Last().MarketData.OrderTopSell, 8));

            double delta = GetPriceDelta(tickers);

            rs.variables.Add("priceDelta", new ResultSet.Variable("Price Delta", delta, 8));

            SaveResult(rs);
        }

        private double GetPriceDelta (TickerChangedEventArgs[] tickers) {

            double endPrice = tickers.Last().MarketData.OrderTopBuy;
            double startPrice = endPrice;

            long endTime = tickers.Last().Timestamp - StartTime;
            long startTime = endTime - Timeframe;

            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp > endTime) endPrice = tickers[i].MarketData.OrderTopBuy;
                if (tickers[i].Timestamp < startTime) break;
                startPrice = tickers[i].MarketData.OrderTopBuy;
            }

            return ((endPrice - startPrice) / startPrice) * 100;
        }

    }
}

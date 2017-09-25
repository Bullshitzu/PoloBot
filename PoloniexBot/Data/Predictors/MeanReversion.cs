using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PoloniexBot.Data.Predictors {
    class MeanReversion : Predictor {

        public MeanReversion (CurrencyPair pair, int period = 12600) : base(pair) {
            localPeriod = period;
        }
        public override void SignResult (ResultSet rs) {
            rs.signature = "Mean Rev.";
        }

        // -------------------
        // Setup Vars
        // -------------------

        private long localPeriod = 12600; // 3.5 hours

        // -------------------

        public void Recalculate (TickerChangedEventArgs[] tickers) {
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            double meanPrice = CalculateMeanPrice(tickers, localPeriod);
            double currPrice = tickers.Last().MarketData.OrderTopBuy;

            double ratio = ((meanPrice - currPrice) / meanPrice) * 100;
            if (double.IsNaN(ratio) || double.IsInfinity(ratio)) ratio = 0;

            rs.variables.Add("price", new ResultSet.Variable("Price", currPrice, 8));
            rs.variables.Add("score", new ResultSet.Variable("Score", ratio, 4));

            SaveResult(rs);
        }

        private double CalculateMeanPrice (TickerChangedEventArgs[] tickers, long timePeriod) {

            long currTime = tickers.Last().Timestamp;
            long startTime = currTime - timePeriod;

            int startIndex = tickers.Length - 1;
            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                startIndex = i;
            }

            double sum = 0;
            int sumCount = 0;
            for (int i = startIndex; i < tickers.Length; i++) {
                sum += tickers[i].MarketData.PriceLast;
                sumCount++;
            }

            return sum / sumCount;
        }

    }
}

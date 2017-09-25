using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PoloniexBot.Data.Predictors {
    class RSI : Predictor {

        public RSI (CurrencyPair pair) : base(pair) { }
        public override void SignResult (ResultSet rs) {
            rs.signature = "R.S.I.";
        }

        // -------------------
        // Setup Vars
        // -------------------

        const long RSITimespan = 900;

        // -------------------

        public void Recalculate (TickerChangedEventArgs[] tickers) {
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("price", new ResultSet.Variable("Price", tickers.Last().MarketData.PriceLast, 8));

            // Calculate RSI
            double RSI = CalculateRSI(tickers);
            if (double.IsNaN(RSI) || double.IsInfinity(RSI)) RSI = 50;
            rs.variables.Add("rsi", new ResultSet.Variable("RSI", RSI, 4));

            SaveResult(rs);
        }

        private double CalculateRSI (TickerChangedEventArgs[] tickers) {

            // find start index
            long rsiStartTime = tickers.Last().Timestamp - RSITimespan;
            int rsiStartIndex = tickers.Length - 1;
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < rsiStartTime) break;
                rsiStartIndex = i;
            }

            // compile the prices since start index to now
            List<double> prices = new List<double>();
            for (int i = rsiStartIndex; i < tickers.Length; i++) {
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            // calculate RSI based on prices
            return Analysis.Other.RelativeStrenghtIndex(prices.ToArray());
        }

    }
}

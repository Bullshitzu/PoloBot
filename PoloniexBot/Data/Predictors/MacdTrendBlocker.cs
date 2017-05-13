using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class MacdTrendBlocker : Predictor {

        static int[] Settings = { 10, 50 };

        public MacdTrendBlocker (CurrencyPair pair) : base(pair) { }
        public override void SignResult (ResultSet rs) {
            rs.signature = "M.A.C.D. Trade";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double smaShort = GetSMA(tickers, Settings[0]);
            double smaLong = GetSMA(tickers, Settings[1]);

            double result = smaShort - smaLong;
            
            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("smaShort", new ResultSet.Variable("SMA (" + Settings[0] + ")", smaShort, 8));
            rs.variables.Add("smaLong", new ResultSet.Variable("SMA (" + Settings[1] + ")", smaLong, 8));
            rs.variables.Add("macd", new ResultSet.Variable("MACD", result, 8));
            
            if (results.Count == 0) SaveResult(rs);
            else SaveResult(rs);

        }

        private double GetSMA (TickerChangedEventArgs[] tickers, int time) {

            int startIndex = tickers.Length - time;
            if (startIndex < 0) startIndex = 0;

            List<double> prices = new List<double>();
            for (int i = tickers.Length - 1; i >= startIndex; i--) {
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            prices.Reverse();
            return Analysis.MovingAverage.SimpleMovingAverage(prices.ToArray());
        }

        // todo: drawing

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class DirectionIndex : Predictor {

        public DirectionIndex (CurrencyPair pair, long smaTimeframe = 1800) : base(pair) {
            this.SMATimeframe = smaTimeframe;
            this.DeltaTimeframe1 = 300; // 5 minutes
            this.DeltaTimeframe2 = 900; // 15 minutes
            this.DeltaTimeframe3 = 1800; // 30 minutes
        }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Dir. Index";
        }

        // ------------------------------------------

        long SMATimeframe;
        long DeltaTimeframe1;
        long DeltaTimeframe2;
        long DeltaTimeframe3;

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double SMA = GetSMA(tickers, SMATimeframe);

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("sma", new ResultSet.Variable("SMA", SMA, 8));

            SaveResult(rs);

            double delta1 = GetDelta(DeltaTimeframe1);
            double delta2 = GetDelta(DeltaTimeframe2, DeltaTimeframe1);
            double delta3 = GetDelta(DeltaTimeframe3, DeltaTimeframe2);

            rs.variables.Add("delta1", new ResultSet.Variable("Delta 1", delta1, 8));
            rs.variables.Add("delta2", new ResultSet.Variable("Delta 2", delta2, 8));
            rs.variables.Add("delta3", new ResultSet.Variable("Delta 3", delta3, 8));

        }

        private double GetSMA (TickerChangedEventArgs[] tickers, long timeframe) {

            long startTime = tickers.Last().Timestamp - timeframe;
            List<double> prices = new List<double>();

            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            prices.Reverse();

            return Analysis.MovingAverage.SimpleMovingAverage(prices.ToArray());
        }

        private double GetDelta (long deltaTimeframe, long endTimeOffset = 0) {
            ResultSet[] results = GetAllResults();

            long startTime = results.Last().timestamp - endTimeOffset - deltaTimeframe;
            ResultSet.Variable tempVar;

            double startPrice = 0;
            double endPrice = 0;

            if (results.Last().variables.TryGetValue("sma", out tempVar)) endPrice = tempVar.value;

            for (int i = results.Length - 1; i >= 0; i--) {
                if (results[i].timestamp < startTime) break;
                if (results[i].variables.TryGetValue("sma", out tempVar)) startPrice = tempVar.value;
            }

            return ((endPrice - startPrice) / startPrice) * 100;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class ADX : Predictor {

        private int localPeriod = 60;

        public ADX (CurrencyPair pair, int period = 60) : base(pair) {
            localPeriod = period;
        }

        public override void SignResult (ResultSet rs) {
            rs.signature = "A.D.X.";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double upMove = GetUpMove(tickers);
            double downMove = GetDownMove(tickers);

            double dmPos = 0;
            double dmNeg = 0;

            if (upMove < 0 && downMove < 0) dmPos = dmNeg = 0;
            else if (upMove > downMove) {
                dmPos = upMove;
                dmNeg = 0;
            }
            else {
                dmPos = 0;
                dmNeg = downMove;
            }

            double[] atrVars = GetATRVars(tickers);
            double atr = Analysis.Other.AverageTrueRange(atrVars[0], atrVars[1], atrVars[2]);

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            SaveResult(rs);
            // note: it needs to be saved immediately since it's used by EMA's
            // (doesn't matter for adding vars, it's a reference...)

            rs.variables.Add("dmPos", new ResultSet.Variable("+DM", dmPos, 8));
            rs.variables.Add("dmNeg", new ResultSet.Variable("-DM", dmNeg, 8));
            rs.variables.Add("atr", new ResultSet.Variable("A.T.R.", atr, 8));

            // now +dm14, -dm14, tr14

            double diPos = 0;
            double diNeg = 0;

            if (results.Count > 0) {
                long startTime = tickers.Last().Timestamp - (localPeriod * 14);
                // note: wilders recommends 14 periods but i like round numbers

                List<double> dmPosVars = new List<double>();
                List<double> dmNegVars = new List<double>();
                List<double> atr14Vars = new List<double>();

                for (int i = results.Count - 1; i >= 0; i--) {
                    if (results[i].timestamp < startTime) break;

                    ResultSet.Variable tempVar;
                    if (results[i].variables.TryGetValue("dmPos", out tempVar)) dmPosVars.Add(tempVar.value);
                    if (results[i].variables.TryGetValue("dmNeg", out tempVar)) dmNegVars.Add(tempVar.value);
                    if (results[i].variables.TryGetValue("atr", out tempVar)) atr14Vars.Add(tempVar.value);
                }

                atr14Vars.Reverse();

                double atr14 = (Analysis.MovingAverage.ExponentialMovingAverageWilders(atr14Vars.ToArray()));

                dmPosVars.Reverse();
                dmNegVars.Reverse();

                diPos = Analysis.MovingAverage.ExponentialMovingAverageWilders(dmPosVars.ToArray()) / atr14;
                diNeg = Analysis.MovingAverage.ExponentialMovingAverageWilders(dmNegVars.ToArray()) / atr14;

            }

            // -----------------

            double dx = Math.Abs(diPos - diNeg) / (diPos + diNeg);

            rs.variables.Add("diPos", new ResultSet.Variable("+DI", diPos, 8));
            rs.variables.Add("diNeg", new ResultSet.Variable("-DI", diNeg, 8));
            rs.variables.Add("dx", new ResultSet.Variable("DX", dx, 8));

            // -----------------

            double adx = 100;

            if (results.Count > 0) {
                long startTime = tickers.Last().Timestamp - localPeriod;

                List<double> DXVars = new List<double>();

                for (int i = results.Count - 1; i >= 0; i--) {
                    if (results[i].timestamp < startTime) break;

                    ResultSet.Variable tempVar;
                    if (results[i].variables.TryGetValue("dx", out tempVar)) DXVars.Add(tempVar.value);
                }

                DXVars.Reverse();
                adx = 100 * Analysis.MovingAverage.ExponentialMovingAverageWilders(DXVars.ToArray());
            }

            rs.variables.Add("adx", new ResultSet.Variable("A.D.X.", adx, 8));
        }

        private double GetUpMove (TickerChangedEventArgs[] tickers) {

            long startTime1 = tickers.Last().Timestamp - localPeriod;
            long startTime2 = startTime1 - localPeriod;

            double todayHigh = tickers.Last().MarketData.PriceLast;
            double yestHigh = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime2) break;
                if (tickers[i].Timestamp < startTime1) {
                    if (tickers[i].MarketData.PriceLast > yestHigh) yestHigh = tickers[i].MarketData.PriceLast;
                }
                else {
                    if (tickers[i].MarketData.PriceLast > todayHigh) todayHigh = tickers[i].MarketData.PriceLast;
                }
            }

            return todayHigh - yestHigh;
        }
        private double GetDownMove (TickerChangedEventArgs[] tickers) {

            long startTime1 = tickers.Last().Timestamp - localPeriod;
            long startTime2 = startTime1 - localPeriod;

            double todayLow = tickers.Last().MarketData.PriceLast;
            double yestLow = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime2) break;
                if (tickers[i].Timestamp < startTime1) {
                    if (tickers[i].MarketData.PriceLast < yestLow) yestLow = tickers[i].MarketData.PriceLast;
                }
                else {
                    if (tickers[i].MarketData.PriceLast < todayLow) todayLow = tickers[i].MarketData.PriceLast;
                }
            }

            return yestLow - todayLow;
        }

        private double[] GetATRVars (TickerChangedEventArgs[] tickers) {

            long startTime = tickers.Last().Timestamp - localPeriod;

            double high = tickers.Last().MarketData.PriceLast;
            double low = tickers.Last().MarketData.PriceLast;
            double previousClose = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                previousClose = tickers[i].MarketData.PriceLast;
                if (tickers[i].Timestamp < startTime) break;
                if (tickers[i].MarketData.PriceLast > high) high = tickers[i].MarketData.PriceLast;
                if (tickers[i].MarketData.PriceLast < low) low = tickers[i].MarketData.PriceLast;
            }

            return new double[] { high, low, previousClose };
        }
    }
}

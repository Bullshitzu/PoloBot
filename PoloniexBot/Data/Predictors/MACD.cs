using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class MACD : Predictor {

        public static int HistogramValueCount = 30;

        private int localTimeEMA = 3000;
        private int localTimeSMA = 3000;

        public MACD (CurrencyPair pair, int timeEMA = 1800) : base(pair) {
            localTimeEMA = timeEMA;
            localTimeSMA = (int)(timeEMA * 1.2);
        }

        public override void SignResult (ResultSet rs) {
            rs.signature = "M.A.C.D.";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double ema = GetSMA(tickers, localTimeEMA);
            double sma = GetSMA(tickers, localTimeSMA);

            double macd = ((ema - sma) / sma) * 100;

            // get the histogram delta (trend)

            double macdTrend = 0;
            if (results != null && results.Count > HistogramValueCount) {
                List<double> macdValues = new List<double>();
                for (int i = results.Count - 1; i >= 0; i--) {

                    if (i < 0) break;
                    if (results[i].timestamp < tickers.Last().Timestamp - localTimeSMA) break;

                    ResultSet.Variable rsTemp;
                    if (results[i].variables.TryGetValue("macd", out rsTemp)) {
                        macdValues.Add(rsTemp.value);
                    }
                }
                double macdSMA = Analysis.MovingAverage.SimpleMovingAverage(macdValues.ToArray());
                macdTrend = macd - macdSMA;
            }

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("emaShort", new ResultSet.Variable("EMA (" + localTimeEMA + ")", ema, 8));
            rs.variables.Add("smaLong", new ResultSet.Variable("SMA (" + localTimeSMA + ")", sma, 8));
            rs.variables.Add("macd", new ResultSet.Variable("MACD", macd, 8));
            rs.variables.Add("macdTrend", new ResultSet.Variable("MACD (Trend)", macdTrend, 8));
            
            SaveResult(rs);

        }

        private double GetEMA (TickerChangedEventArgs[] tickers, int time) {
            long startTime = tickers.Last().Timestamp - time;

            List<double> prices = new List<double>();
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            prices.Reverse();
            return Analysis.MovingAverage.ExponentialMovingAverage(prices.ToArray());
        }
        private double GetSMA (TickerChangedEventArgs[] tickers, int time) {
            long startTime = tickers.Last().Timestamp - time;

            List<double> prices = new List<double>();
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            prices.Reverse();
            return Analysis.MovingAverage.SimpleMovingAverage(prices.ToArray());
        }
    }
}

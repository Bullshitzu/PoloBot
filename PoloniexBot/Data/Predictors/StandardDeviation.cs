using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class StandardDeviation : Predictor {

        private long localPeriod;

        public StandardDeviation (CurrencyPair pair, int period = 12600) : base(pair) {
            localPeriod = period;
        }

        public override void SignResult (ResultSet rs) {
            rs.signature = "St.Dev.";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double avg = GetAverage(tickers);
            double stDev = GetStDev(tickers, avg);

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("sma", new ResultSet.Variable("SMA", avg, 8));
            rs.variables.Add("stDev", new ResultSet.Variable("St.Dev.", stDev, 8));

            SaveResult(rs);
        }

        private double GetAverage (TickerChangedEventArgs[] tickers) {

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - localPeriod;

            double sum = 0;
            int cnt = 0;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                sum += tickers[i].MarketData.PriceLast;
                cnt++;
            }

            return sum / cnt;
        }

        private double GetStDev (TickerChangedEventArgs[] tickers, double avg) {

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - localPeriod;

            double sum = 0;
            int cnt = 0;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                sum += Math.Abs(avg - tickers[i].MarketData.PriceLast);
                cnt++;
            }

            return sum / cnt;
        }
    }
}

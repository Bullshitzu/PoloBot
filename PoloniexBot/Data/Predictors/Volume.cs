using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class Volume : Predictor {

        private long timespanShort;
        private long timespanLong;

        public Volume (CurrencyPair pair, long timespanShort, long timespanLong) : base(pair) {
            this.timespanShort = timespanShort;
            this.timespanLong = timespanLong;
        }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Volume";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double shortVolume = GetAvgTransactionCount(tickers, timespanShort);
            double longVolume = GetAvgTransactionCount(tickers, timespanLong);

            double r = shortVolume / longVolume;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("ratio", new ResultSet.Variable("Ratio", r, 8));
            
            SaveResult(rs);
        }

        private double GetAvgTransactionCount (TickerChangedEventArgs[] tickers, long timeframe) {
            if (tickers == null) return 0;

            long breakTimestamp = tickers.Last().Timestamp - timeframe;

            int cnt = 0;
            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < breakTimestamp) break;
                cnt++;
            }

            return ((double)cnt / timeframe) * 60;
        }
    }
}

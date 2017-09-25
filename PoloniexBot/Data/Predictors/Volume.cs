using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class Volume : Predictor {

        const int Period = 60;
        const int PeriodInterval = 10;

        static int[] Settings = { 30, 300 };

        private double avgTradesPerSecond = 0;
        private long[] timespans = { 120, 7200 }; // these don't really matter, they're overwritten in Setup (calculated per pair individually)

        public Volume (CurrencyPair pair) : base(pair) { }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Volume";
        }

        public void Setup (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            long timescale = tickers.Last().Timestamp - tickers.First().Timestamp;
            avgTradesPerSecond = tickers.Length / (double)timescale;

            long tsShort = (long)((1 / avgTradesPerSecond) * Settings[0] + 1); // note: time in seconds for 10 trades (average)
            long tsLong = (long)((1 / avgTradesPerSecond) * Settings[1] + 1); // note: time in seconds for 75 trades (average)

            timespans = new long[] { tsShort, tsLong };
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            long currTime = tickers.Last().Timestamp;
            ResultSet rs = new ResultSet(currTime);

            double volumeShort = GetVolume(tickers, timespans[0]) / timespans[0];
            double volumeLong = GetVolume(tickers, timespans[1]) / timespans[1];
            double deltaVolume = 0;

            rs.variables.Add("volumeShort", new ResultSet.Variable("Short Volume", volumeShort, 3));
            rs.variables.Add("volumeLong", new ResultSet.Variable("Long Volume", volumeLong, 3));

            if (results != null && results.Count > 5) {
                ResultSet.Variable lastRs;
                double sum = 0;

                for (int i = results.Count-1; i >= 0 && i > results.Count-6; i--) {
                    if (results[i].variables.TryGetValue("volumeShort", out lastRs)) sum += lastRs.value;
                    else {
                        // todo: some kind of error notification
                    }
                }

                deltaVolume = sum / 5;
                deltaVolume = volumeShort / deltaVolume;
            }

            rs.variables.Add("volumeDelta", new ResultSet.Variable("Delta Volume", deltaVolume, 3));

            SaveResult(rs);
        }

        static double GetVolume (TickerChangedEventArgs[] tickers, long timespan) {
            if (tickers == null || tickers.Length == 0) return 0;

            long lastTimestamp = tickers.Last().Timestamp;
            long firstTimestamp = lastTimestamp - timespan;

            double sum = 0;

            float mult = 1f / (lastTimestamp - firstTimestamp);

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < firstTimestamp) break;

                float factor = (lastTimestamp - tickers[i].Timestamp) * mult;
                factor = 1 - factor;
                sum += factor;
            }

            return sum;
        }
    }
}

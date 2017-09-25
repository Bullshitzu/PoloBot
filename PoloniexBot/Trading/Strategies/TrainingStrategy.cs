using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexBot.Data.Predictors;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    class TrainingStrategy : IDisposable {

        public TrainingStrategy (CurrencyPair pair) {
            this.pair = pair;
        }

        public CurrencyPair pair;

        // -------------------------------------

        MACD[] predictorsMacd;
        MeanReversion[] predictorsMeanRev;
        Data.Predictors.PriceDelta[] predictorsPriceDelta;

        public static int[] macdPeriods = { 600, 1200, 2400, 3600 };
        public static int[] meanRevPeriods = { 600, 900, 1200, 1800, 3600, 7200, 10800, 14400 };
        public static int[] priceDeltaPeriods = { 600, 1200, 3600, 7200, 10800, 14400 };

        // -------------------------------------

        public void GeneratePredictors () {

            predictorsMacd = new MACD[macdPeriods.Length];
            for (int i = 0; i < macdPeriods.Length; i++) {
                predictorsMacd[i] = new MACD(pair, macdPeriods[i]);
            }

            predictorsMeanRev = new MeanReversion[meanRevPeriods.Length];
            for (int i = 0; i < meanRevPeriods.Length; i++) {
                predictorsMeanRev[i] = new MeanReversion(pair, meanRevPeriods[i]);
            }

            predictorsPriceDelta = new Data.Predictors.PriceDelta[priceDeltaPeriods.Length];
            for (int i = 0; i < priceDeltaPeriods.Length; i++) {
                predictorsPriceDelta[i] = new Data.Predictors.PriceDelta(pair, priceDeltaPeriods[i]);
            }

        }

        public double[] Recalculate () {
            
            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) return null;

            List<double> results = new List<double>();

            Data.ResultSet.Variable tempVar;
            /*
            for (int i = 0; i < predictorsMacd.Length; i++) {
                predictorsMacd[i].Recalculate(tickers);
                if (predictorsMacd[i].GetLastResult().variables.TryGetValue("macd", out tempVar)) results.Add(tempVar.value);
                else results.Add(0);
            }
            */
            for (int i = 0; i < predictorsMeanRev.Length; i++) {
                predictorsMeanRev[i].Recalculate(tickers);
                if (predictorsMeanRev[i].GetLastResult().variables.TryGetValue("score", out tempVar)) results.Add(tempVar.value);
                else results.Add(0);
            }
            /*
            for (int i = 0; i < predictorsPriceDelta.Length; i++) {
                predictorsPriceDelta[i].Recalculate(tickers);
                if (predictorsPriceDelta[i].GetLastResult().variables.TryGetValue("priceDelta", out tempVar)) results.Add(tempVar.value);
                else results.Add(0);
            }
            */
            return results.ToArray();
        }

        public void Dispose () {

            for (int i = 0; i < predictorsMeanRev.Length; i++) {
                predictorsMeanRev[i].Dispose();
                predictorsMeanRev[i] = null;
            }

            for (int i = 0; i < predictorsMacd.Length; i++) {
                predictorsMacd[i].Dispose();
                predictorsMacd[i] = null;
            }

            for (int i = 0; i < predictorsPriceDelta.Length; i++) {
                predictorsPriceDelta[i].Dispose();
                predictorsPriceDelta[i] = null;
            }

            predictorsMacd = null;
            predictorsMeanRev = null;
            predictorsPriceDelta = null;

        }
    }
}

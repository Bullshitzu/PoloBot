using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot {
    public static class Training {

        public static void Train () {
            CurrencyPair[] pairs = Data.Store.GetAvailableTickerPairs();
            for (int i = 0; i < pairs.Length; i++) {
                TrainPair(pairs[i]);
            }
        }

        private static void TrainPair (CurrencyPair pair) {

            CLI.Manager.PrintLog("Logging patterns from " + pair);
            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);

            


        }

        private static void MakePatternTrainPass (TickerChangedEventArgs[] tickers, int size) {

            for (int i = 0; i < tickers.Length - size; i++) {

                List<TickerChangedEventArgs> currPattern = new List<TickerChangedEventArgs>();
                for (int j = i; j < i + size; j++) {
                    currPattern.Add(tickers[j]);
                }

                int predictionIndex = (int)((i + size) + (size * 0.33f));
                if (predictionIndex >= tickers.Length) predictionIndex = tickers.Length - 1;

                Data.PatternMatching.MapPattern(currPattern.ToArray(), tickers[predictionIndex]);

                if (i % 1000 == 0) Console.WriteLine("Mapping: " + i + "/" + tickers.Length);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.TriArbitrage {
    public static class Manager {

        private const string base1 = "BTC";
        private const string base2 = "ETH";

        private static List<PairMonitor> PairMonitors;

        public static CurrencyPair[] GetTradePairs () {
            
            CurrencyPair[] allPairs = Data.Store.MarketData.Keys.ToArray();

            List<CurrencyPair> pairsBase1 = new List<CurrencyPair>();
            List<CurrencyPair> pairsBase2 = new List<CurrencyPair>();

            for (int i = 0; i < allPairs.Length; i++) {
                if (allPairs[i].BaseCurrency == base1) pairsBase1.Add(allPairs[i]);
                if (allPairs[i].BaseCurrency == base2) pairsBase2.Add(allPairs[i]);
            }

            List<string> commonPairs = new List<string>();
            for (int i = 0; i < pairsBase1.Count; i++) {
                for (int j = 0; j < pairsBase2.Count; j++) {
                    if (pairsBase1[i].QuoteCurrency == pairsBase2[j].QuoteCurrency) {
                        commonPairs.Add(pairsBase1[i].QuoteCurrency);
                        break;
                    }
                }
            }

            List<CurrencyPair> tradePairs = new List<CurrencyPair>();
            PairMonitors = new List<PairMonitor>();

            for (int i = 0; i < commonPairs.Count; i++) {
                tradePairs.Add(new CurrencyPair(base1, commonPairs[i]));
                tradePairs.Add(new CurrencyPair(base2, commonPairs[i]));
                PairMonitors.Add(new PairMonitor(commonPairs[i], base1, base2));
            }

            return tradePairs.ToArray();
        }

    }
}

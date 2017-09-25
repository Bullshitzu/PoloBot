using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PoloniexAPI;

namespace PoloniexBot.Data {
    public static class VarAnalysis {

        private static double[] priceDeltaValues = { 0.025, 0.05, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7 };

        public static void AnalyzeAll () {

            CurrencyPair[] allPairs = Data.Store.MarketData.Keys.ToArray();

            Console.WriteLine(allPairs.Length);

            for (int i = 0; i < allPairs.Length; i++) {
                if (allPairs[i].BaseCurrency != "BTC") continue;

                Console.WriteLine("Pulling data for " + allPairs[i]);

                // pull 3 days worth of price data
                Data.Store.ClearTickerData();
                Data.Store.PullTickerHistory(allPairs[i], 24 * 7);

                // save price data
                Data.Store.SaveTradeData();

                // for each possible trigger value
                double bestScore = double.MinValue;
                for (int j = 0; j < priceDeltaValues.Length; j++) {
                    try {
                        Console.WriteLine("Testing " + allPairs[i] + " with " + priceDeltaValues[j].ToString("F2"));

                        // assign price delta triggers (values above are trigger1; trigger2 = trigger1 * 3; trigger3 = trigger1 * 6)
                        Trading.Rules.RulePriceDelta.Trigger1 = priceDeltaValues[j] * 1;
                        Trading.Rules.RulePriceDelta.Trigger2 = priceDeltaValues[j] * 3;
                        Trading.Rules.RulePriceDelta.Trigger3 = priceDeltaValues[j] * 6;

                        // simulate with these settings
                        Simulation.SimulateAll();

                        // get result; if better save the settings for this pair
                        KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] balances = PoloniexBot.ClientManager.client.Wallet.GetBalancesAsync().Result.ToArray();

                        double btcSum = 0;
                        for (int z = 0; z < balances.Length; z++) {
                            btcSum += balances[z].Value.BitcoinValue;
                        }
                        if (btcSum > bestScore) {
                            SaveResults(allPairs[i], priceDeltaValues[j], btcSum);
                            bestScore = btcSum;
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    }
                }
            }
        }

        public class VarPairData {

            public long timestamp;
            public double deltaValue;
            public double result;

            public VarPairData (long timestamp, double deltaValue, double result) {
                this.timestamp = timestamp;
                this.deltaValue = deltaValue;
                this.result = result;
            }

        }

        private static void SaveResults (CurrencyPair pair, double priceDeltaValue, double result) {

            List<string> lines = new List<string>();

            lines.Add(Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            lines.Add(priceDeltaValue.ToString("F8", System.Globalization.CultureInfo.InvariantCulture));
            lines.Add(result.ToString("F8", System.Globalization.CultureInfo.InvariantCulture));

            Utility.FileManager.SaveFile("data/ov_" + pair + ".data", lines.ToArray());
        }

        public static VarPairData LoadResults (CurrencyPair pair) {

            string filename = "data/ov_" + pair + ".data";

            string[] lines = Utility.FileManager.ReadFile(filename);
            if (lines == null) return null;

            long timestamp = long.Parse(lines[0]);
            double deltaValue = double.Parse(lines[1], System.Globalization.CultureInfo.InvariantCulture);
            double result = double.Parse(lines[2], System.Globalization.CultureInfo.InvariantCulture);

            return new VarPairData(timestamp, deltaValue, result);
        }

        public static KeyValuePair<CurrencyPair, double>[] GetBestCurrencyPairs () {

            List<KeyValuePair<CurrencyPair, double>> data = new List<KeyValuePair<CurrencyPair, double>>();

            List<string> ovFiles = new List<string>();
            List<string> allFiles = new List<string>(Directory.GetFiles("data"));
            for (int i = 0; i < allFiles.Count; i++) {
                string filename = allFiles[i].Split('\\')[1];
                if (filename.StartsWith("ov_")) {

                    string pairName = filename.Substring(3).Split('.')[0];

                    string[] lines = Utility.FileManager.ReadFile(allFiles[i]);
                    if (lines == null) continue;

                    double result = double.Parse(lines[2], System.Globalization.CultureInfo.InvariantCulture);

                    data.Add(new KeyValuePair<CurrencyPair, double>(CurrencyPair.Parse(pairName), result));
                }
            }

            data.Sort(new Utility.MarketDataComparerTrend());
            data.Reverse();

            return data.ToArray();
        }
    }
}

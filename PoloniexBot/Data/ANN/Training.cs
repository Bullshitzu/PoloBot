using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PoloniexBot.Data.PatternMatching;
using PoloniexAPI;

namespace PoloniexBot.Data.ANN {
    class Training {

        public static void RebuildAllNetworks () {

            Manager.LoadFromFile();
            List<KeyValuePair<CurrencyPair, List<Pattern>>> repo = Manager.Repo;

            for (int i = 0; i < repo.Count; i++) {

                CurrencyPair pair = repo[i].Key;
                List<Pattern> patterns = repo[i].Value;

                string filename = "data/" + pair + ".ann";

                Network net = new Network(new int[] { 8, 9, 1 }, double.MaxValue);

                Console.WriteLine("Optimizing ANN for " + pair + " (" + patterns.Count + " patterns)");
                double avgError = OptimizeGenetic(net, patterns, pair);
                Console.WriteLine("Optimization done! Saving ANN to " + filename);

                string[] lines = net.ToStringLines(avgError);

                Utility.FileManager.SaveFile(filename, lines);

            }
        }

        private static double OptimizeGenetic (Network net, List<Pattern> patterns, CurrencyPair pair) {
            // returns best average error

            double bestError = double.MaxValue;
            int iterationsWithoutImprovement = 0;

            while (iterationsWithoutImprovement < 100) {

                double totalError = 0;

                for (int j = 0; j < patterns.Count; j++) {

                    net.SetInputs(patterns[j].movement, true);
                    net.Recalculate();
                    double output = net.GetOutputs()[0];

                    totalError += Math.Abs(output - patterns[j].movement.Last());

                }

                if (totalError < bestError) {
                    iterationsWithoutImprovement = 0;
                    bestError = totalError;

                    double avgError = bestError / patterns.Count;

                    Console.WriteLine("New best for " + pair + ": " + avgError.ToString("F4"));
                }
                else {
                    iterationsWithoutImprovement++;
                    net.Revert();
                }

                net.Mutate();
            }

            net.Revert();

            return bestError / patterns.Count;
        }

        public static List<KeyValuePair<CurrencyPair, double>> GetNetworkAccuracy () {

            string[] files = Directory.GetFiles("data");
            for (int i = 0; i < files.Length; i++) {
                files[i] = files[i].Replace('\\', '/');
            }

            List<KeyValuePair<CurrencyPair, double>> networkErrors = new List<KeyValuePair<CurrencyPair, double>>();
            for (int i = 0; i < files.Length; i++) {
                if (files[i].EndsWith(".ann")) {

                    CurrencyPair pair = CurrencyPair.Parse(files[i].Split('/').Last().Split('.').First());
                    double avgError = double.Parse(Utility.FileManager.ReadFile(files[i]).First(), System.Globalization.CultureInfo.InvariantCulture);

                    networkErrors.Add(new KeyValuePair<CurrencyPair, double>(pair, avgError));
                }
            }

            networkErrors.Sort(new Utility.MarketDataComparerTrend());
            return networkErrors;
        }
    }
}

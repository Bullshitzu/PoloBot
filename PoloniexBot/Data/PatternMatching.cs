using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data {
    public static class PatternMatching {

        private const int MinPatternPoints = 4;
        private const int MaxPatternPoints = 8;
        private const double MinPatternDelta = 0.5; // minimum total delta in the pattern to be valid (in %)
        private const double MinPatternDeltaStep = 0.001; // minimum step between two prices

        private const double PatternDistanceThreshold = 0.002;

        private const string FilePath = "data/patterns";

        private static List<Pattern> PatternRepo;

        public class Pattern {
            
            public double[] movement;
            public double prediction;

            public Pattern (double[] movement) {
                this.movement = movement;
                prediction = 0;
            }
            public Pattern (double[] movement, double prediction) {
                this.movement = movement;
                this.prediction = prediction;
            }

            public double GetDistance (Pattern other) {

                if (other.movement.Length < this.movement.Length)
                    return other.GetDistance(this);

                double errorSum = 0;
                for (int i = 0; i < movement.Length; i++) {
                    errorSum += Math.Abs(movement[i] - other.movement[i]);
                }
                return errorSum / movement.Length;
            }

            public override string ToString () {
                string line = movement.Length + ";";
                for (int i = 0; i < movement.Length; i++) {
                    line += movement[i].ToString("F8") + ";";
                }
                line += prediction.ToString("F8");
                return line;
            }

            public static Pattern Parse (string text) {
                string[] parts = text.Split(';');

                int movementNum = int.Parse(parts[0]);
                double[] mov = new double[movementNum];

                for (int i = 0; i < movementNum; i++) {
                    mov[i] = double.Parse(parts[i + 1]);
                }

                double pred = double.Parse(parts[parts.Length - 1]);

                Pattern p = new Pattern(mov, pred);
                return p;
            }
        }
        private struct PatternComparer : IComparable<PatternComparer> {
            public Pattern pattern;
            public double difference;

            public PatternComparer (Pattern pattern, double difference) {
                this.pattern = pattern;
                this.difference = difference;
            }

            public int CompareTo (PatternComparer other) {
                return difference.CompareTo(other.difference);
            }
        }

        public static void MapPattern (TickerChangedEventArgs[] tickers, TickerChangedEventArgs prediction) {
            double[] values = new double[tickers.Length];
            for (int i = 0; i < tickers.Length; i++) {
                values[i] = tickers[i].MarketData.PriceLast;
            }
            MapPattern(values, prediction.MarketData.PriceLast);
        }
        public static void MapPattern (double[] prices, double prediction) {
            Pattern p = GeneratePattern(prices, prediction);
            if (p != null) AddPattern(p);
        }
        private static void AddPattern (Pattern p) {
            if (PatternRepo == null) PatternRepo = new List<Pattern>();

            // compare to existing patterns to avoid duplicates
            bool hasDuplicate = false;
            for (int i = 0; i < PatternRepo.Count; i++) {
                double d = PatternRepo[i].GetDistance(p);

                if (d < PatternDistanceThreshold) { // they're the same
                    hasDuplicate = true;
                    break;
                }
            }

            if (!hasDuplicate) {
                PatternRepo.Add(p);
            }
        }

        private static Pattern GeneratePattern (double[] prices, double prediction) {
            
            // get price SMA
            double[] sma = GetSMA(prices);

            // find local extremes
            List<int> maxIndexes = new List<int>();
            List<int> minIndexes = new List<int>();
            MapExtremes(sma, maxIndexes, minIndexes);

            // map the movement between extremes
            double[] deltas = MapDeltas(prices, maxIndexes.ToArray(), minIndexes.ToArray());

            // normalize the movement (to % delta)
            // i.e. make it price independant - applicable to all pairs
            deltas = NormalizeDeltas(prices[0], deltas);

            if (VerifyPattern(deltas)) {

                // normalize the prediction to percent delta
                prediction = (prediction - prices[0]) / prices[0];

                return new Pattern(deltas, prediction);
            }

            return null;
        }

        public static Pattern[] FindMatch (TickerChangedEventArgs[] tickers, int matchCount) {
            double[] values = new double[tickers.Length];
            for (int i = 0; i < tickers.Length; i++) {
                values[i] = tickers[i].MarketData.PriceLast;
            }
            return FindMatch(values, matchCount);
        }
        public static Pattern[] FindMatch (double[] prices, int matchCount) {

            // turn prices into a pattern
            // prediction doesn't matter, it's not used
            Pattern p = GeneratePattern(prices, 0);
            if (p == null) return null;

            // create comparers with all existing patterns
            List<PatternComparer> comparers = new List<PatternComparer>();
            for (int i = 0; i < PatternRepo.Count; i++) {
                double diff = p.GetDistance(PatternRepo[i]);
                comparers.Add(new PatternComparer(PatternRepo[i], diff));
            }

            // sort the comparers list
            comparers.Sort();

            // extract N (matchCount) most similar patterns
            List<Pattern> closestPatterns = new List<Pattern>();
            for (int i = 0; i < matchCount && i < comparers.Count; i++) {
                closestPatterns.Add(comparers[i].pattern);
            }

            return closestPatterns.ToArray();
        }

        #region Private Utility Functions
        private static bool VerifyPattern (double[] deltas) {
            if (deltas == null) return false;

            if (deltas.Length < MinPatternPoints) return false;
            if (deltas.Length > MaxPatternPoints) return false;

            double totalDelta = 0;
            for (int i = 0; i < deltas.Length; i++) {
                totalDelta += Math.Abs(deltas[i]);
            }
            if (totalDelta * 100 < MinPatternDelta) return false;

            return true;
        }

        private static double[] NormalizeDeltas (double basePrice, double[] deltas) {
            if (deltas == null) return null;

            double[] normDeltas = new double[deltas.Length];

            for (int i = 0; i < deltas.Length; i++) {
                normDeltas[i] = deltas[i] / basePrice;
            }

            return normDeltas;
        }
        private static double[] MapDeltas (double[] prices, int[] maxIndexes, int[] minIndexes) {
            if (maxIndexes.Length < 1 || minIndexes.Length < 1) return null;

            List<double> deltas = new List<double>();
            bool rising = minIndexes[0] < maxIndexes[0];

            int minIndex = 0;
            int maxIndex = 0;

            while (true) {
                if (minIndex >= minIndexes.Length || maxIndex >= maxIndexes.Length) break;

                double currDelta;

                if (rising) {
                    currDelta = prices[maxIndexes[maxIndex]] - prices[minIndexes[minIndex]];
                    minIndex++;
                    rising = false;
                }
                else {
                    currDelta = prices[minIndexes[minIndex]] - prices[maxIndexes[maxIndex]];
                    maxIndex++;
                    rising = true;
                }

                deltas.Add(currDelta);
            }

            return deltas.ToArray();
        }
        private static void MapExtremes (double[] prices, List<int> maxIndexes, List<int> minIndexes) {
            // MinPatternDeltaStep

            double currMin = prices[0];
            double currMax = prices[0];

            int currMinIndex = 0;
            int currMaxIndex = 0;

            bool overrideHigh = false;
            bool overrideLow = false;

            for (int i = 0; i < prices.Length; i++) {
                double currPrice = prices[i];

                if (currPrice > currMax) {
                    currMax = currPrice;
                    currMaxIndex = i;
                }

                if (currPrice < currMin) {
                    currMin = currPrice;
                    currMinIndex = i;
                }

                if (overrideHigh && currMaxIndex < currMinIndex) {
                    currMax = currMin;
                    currMaxIndex = currMinIndex;
                }

                if (overrideLow && currMinIndex < currMaxIndex) {
                    currMin = currMax;
                    currMinIndex = currMaxIndex;
                }

                double delta = (currMax - currMin) / currMin;
                if (delta >= MinPatternDeltaStep) {

                    if (currMaxIndex > currMinIndex && !overrideLow) {
                        minIndexes.Add(currMinIndex);

                        overrideLow = true;
                        overrideHigh = false;
                    }
                    else if (currMinIndex > currMaxIndex && !overrideHigh) {
                        maxIndexes.Add(currMaxIndex);

                        overrideHigh = true;
                        overrideLow = false;
                    }
                }
            }
        }
        private static double[] GetSMA (double[] values) {
            double[] data = new double[values.Length];

            int range = 1 + values.Length / 10;

            for (int i = 0; i < values.Length; i++) {
                double sum = 0;
                int cnt = 0;
                for (int j = i; j >= 0 && j >= i - range; j--) {
                    sum += values[j];
                    cnt++;
                }
                data[i] = sum / cnt;
            }

            return data;
        }
        #endregion

        #region File Save / Load
        public static void SaveRepoToFile () {
            if (PatternRepo == null) return;

            List<string> lines = new List<string>();
            for (int i = 0; i < PatternRepo.Count; i++) {
                lines.Add(PatternRepo[i].ToString());
            }

            Utility.FileManager.SaveFile(FilePath, lines.ToArray());
        }
        public static void LoadRepoFromFile () {
            string[] lines = Utility.FileManager.ReadFile(FilePath);

            PatternRepo = new List<Pattern>();

            for (int i = 0; i < lines.Length; i++) {
                Pattern p = Pattern.Parse(lines[i]);
                PatternRepo.Add(p);
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data {
    public static class VariableAnalysis {

        public static void AnalyzeAllPairs () {

            List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> markets =
                new List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>>(Data.Store.MarketData.ToArray());

            markets.Sort(new Utility.MarketDataComparerVolume());
            markets.Reverse();

            for (int i = 0; i < markets.Count; i++) {
                if (markets[i].Key.BaseCurrency == "BTC") {
                    try {
                        DoFullAnalysis(markets[i].Key);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message + " - " + e.StackTrace);
                    }
                }
            }

        }

        public static void DoFullAnalysis (CurrencyPair pair) {

            // pull N days of data and precalculate
            Precalculation.PullAndGenerate(pair);

            Thread.Sleep(1500);

            // perform analysis
            PerformAnalysis(pair);

        }

        private static void PerformAnalysis (CurrencyPair pair) {

            Console.WriteLine("Initiating analysis for " + pair);

            // load precalculated data
            Precalculation.DataPoint[] data = LoadPrecalculatedData(pair);

            if (data == null) throw new Exception("No precalculated data found for " + pair + "!");

            // normalize data results and variables
            Precalculation.DataPoint[] normData = NormalizeData(data);

            // find information gain for each variable
            double[] infGain = FindInformationGain(normData);

            // select the variable with best information gain (lowest value)
            int bestVariableIndex = GetBestVariableIndex(infGain);

            // find optimal value for trigger
            double optimalTrigger = FindOptimalTrigger(data, bestVariableIndex);

            // find last timestamp
            long lastTimestamp = GetLastTimestamp(data);

            // write down the results somewhere to be used by TPManager
            WriteOptimalTrigger(pair, lastTimestamp, Trading.Strategies.TrainingStrategy.meanRevPeriods[bestVariableIndex], optimalTrigger);

        }

        // ------------------------------------------

        public struct OptimizedPairData {

            public CurrencyPair Pair;
            public long Timestamp;

            public long MeanRevTimeframe;
            public double MeanRevTrigger;

            public OptimizedPairData (CurrencyPair pair, long timestamp, long mRevTimeframe, double mRevTrigger) {
                this.Pair = pair;
                this.Timestamp = timestamp;
                this.MeanRevTimeframe = mRevTimeframe;
                this.MeanRevTrigger = mRevTrigger;
            }

            public static OptimizedPairData Parse (string[] lines) {
                if (lines == null || lines.Length != 4) throw new Exception("Error parsing OptimizedPairData!");

                CurrencyPair pair = CurrencyPair.Parse(lines[0]);
                long timestamp = long.Parse(lines[1]);

                long timeframe = long.Parse(lines[2]);
                double trigger = double.Parse(lines[3]);

                return new OptimizedPairData(pair, timestamp, timeframe, trigger);
            }
        }

        public static OptimizedPairData GetPairData (CurrencyPair pair) {
            string filename = "data/opt_" + pair + ".data";
            string[] lines = Utility.FileManager.ReadFile(filename);
            return OptimizedPairData.Parse(lines);
        }

        // ------------------------------------------
        // Private utility functions
        // ------------------------------------------

        private static Precalculation.DataPoint[] LoadPrecalculatedData (CurrencyPair pair) {

            string filename = "data/pc_" + pair + ".data";

            string[] lines = Utility.FileManager.ReadFile(filename);
            if (lines == null) return null;

            List<Precalculation.DataPoint> data = new List<Precalculation.DataPoint>();
            for (int i = 3; i < lines.Length; i++) {
                data.Add(Precalculation.DataPoint.Parse(lines[i]));
            }

            return data.ToArray();

        }

        private static Precalculation.DataPoint[] NormalizeData (Precalculation.DataPoint[] data) {
            if (data == null) return null;

            // find minimums and maximums for all
            double minResult = double.MaxValue;
            double maxResult = double.MinValue;

            double[] minimums = new double[data.First().Data.Length];
            double[] maximums = new double[data.First().Data.Length];

            for (int i = 0; i < minimums.Length; i++) {
                minimums[i] = double.MaxValue;
                maximums[i] = double.MinValue;
            }

            for (int i = 0; i < data.Length; i++) {
                if (data[i].Result > maxResult) maxResult = data[i].Result;
                if (data[i].Result < minResult) minResult = data[i].Result;

                for (int j = 0; j < data[i].Data.Length; j++) {
                    if (data[i].Data[j] > maximums[j]) maximums[j] = data[i].Data[j];
                    if (data[i].Data[j] < minimums[j]) minimums[j] = data[i].Data[j];
                }
            }

            // find ranges for result and each variable
            double rangeResult = maxResult - minResult;
            double[] ranges = new double[minimums.Length];
            for (int i = 0; i < minimums.Length; i++) {
                ranges[i] = maximums[i] - minimums[i];
            }

            Precalculation.DataPoint[] normData = new Precalculation.DataPoint[data.Length];

            // normalize everything within the range
            for (int i = 0; i < data.Length; i++) {
                double normResult = (data[i].Result - minResult) / rangeResult;
                double[] normValues = new double[data[i].Data.Length];
                for (int j = 0; j < normValues.Length; j++) {
                    normValues[j] = (data[i].Data[j] - minimums[j]) / ranges[j];
                }

                normData[i] = new Precalculation.DataPoint(data[i].Pair, data[i].Timestamp, normResult, normValues);
            }

            return normData;
        }

        private static double FindAverageResult (Precalculation.DataPoint[] data) {
            if (data == null) return 0;

            double sum = 0;
            for (int i = 0; i < data.Length; i++) {
                sum += data[i].Result;
            }
            sum /= data.Length;

            return sum;
        }
        private static double[] FindAverageVariables (Precalculation.DataPoint[] data) {

            double[] sums = new double[data.First().Data.Length];
            for (int i = 0; i < data.Length; i++) {
                for (int j = 0; j < sums.Length; j++) {
                    sums[j] += data[i].Data[j];
                }
            }
            for (int i = 0; i < sums.Length; i++) {
                sums[i] /= data.Length;
            }

            return sums;
        }
        private static double[] FindAverageRatios (double avgResult, double[] avgVariables) {
            double[] ratios = new double[avgVariables.Length];

            for (int i = 0; i < avgVariables.Length; i++) {
                ratios[i] = avgResult / avgVariables[i];
            }

            return ratios;
        }
        private static double[] FindStandardRatioDeviations (Precalculation.DataPoint[] data, double[] avgRatios) {

            double[] distances = new double[avgRatios.Length];
            int[] counts = new int[avgRatios.Length];

            for (int i = 0; i < data.Length; i++) {
                for (int j = 0; j < data[i].Data.Length; j++) {
                    double d = Math.Abs(avgRatios[j] - (data[i].Result / data[i].Data[j]));
                    if (!double.IsNaN(d) && !double.IsInfinity(d)) {
                        distances[j] += d;
                        counts[j]++;
                    }
                }
            }
            
            for (int i = 0; i < distances.Length; i++) {
                distances[i] /= counts[i];
            }
            
            return distances;
        }
        private static double[] FindInformationGain (Precalculation.DataPoint[] data) {
            if (data == null) return null;

            // find average result value
            Console.WriteLine("Seeking average result");
            double avgResult = FindAverageResult(data);

            // find average variable values
            Console.WriteLine("Seeking average variables");
            double[] avgVariables = FindAverageVariables(data);

            // find average ratios
            Console.WriteLine("Seeking average ratios");
            double[] avgRatios = FindAverageRatios(avgResult, avgVariables);

            // find standard ratio deviations
            Console.WriteLine("Seeking standard ratio deviations");
            double[] stRatioDev = FindStandardRatioDeviations(data, avgRatios);

            return stRatioDev;
        }
        
        private static int GetBestVariableIndex (double[] infGain) {
            if (infGain == null) return 0;

            int bestIndex = 0;
            double bestValue = double.MaxValue;
            for (int i = 0; i < infGain.Length; i++) {
                if (infGain[i] < bestValue) {
                    bestIndex = i;
                    bestValue = infGain[i];
                }
            }

            return bestIndex;
        }
        private static double FindOptimalTrigger (Precalculation.DataPoint[] data, int varIndex) {
            if (data == null) return 0;

            List<Precalculation.DataPoint> dataList = new List<Precalculation.DataPoint>(data);
            dataList.Sort(new Precalculation.DataPointComparerVariable(varIndex));
            dataList.Reverse();

            double lowestTrigger = double.MaxValue;
            for (int i = 0; i < dataList.Count; i++) {
                if (dataList[i].Data[varIndex] < lowestTrigger) lowestTrigger = dataList[i].Data[varIndex];
                if (dataList[i].Result < 0.5) break;
            }

            return lowestTrigger;

        }
        private static long GetLastTimestamp (Precalculation.DataPoint[] data) {
            if (data == null) return 0;

            List<Precalculation.DataPoint> tempList = new List<Precalculation.DataPoint>(data);
            tempList.Sort();

            return tempList.Last().Timestamp;
        }

        private static void WriteOptimalTrigger (CurrencyPair pair, long lastTimestamp, long mRevTimespan, double mRevTrigger) {

            string filename = "data/opt_" + pair + ".data";

            string[] lines = {
                pair.ToString(),
                lastTimestamp.ToString(),
                mRevTimespan.ToString(),
                mRevTrigger.ToString("F8"),
            };

            Utility.FileManager.SaveFile(filename, lines);

        }
    }
}

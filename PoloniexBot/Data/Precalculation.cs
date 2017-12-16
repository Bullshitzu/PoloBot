using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data {
    public static class Precalculation {

        static Precalculation () {
            wallet = (PoloniexAPI.WalletTools.WalletSimulated)PoloniexBot.ClientManager.client.Wallet;
        }

        static PoloniexAPI.WalletTools.WalletSimulated wallet;
        public static void ResetWallet () {
            wallet.Reset();
        }

        public class DataPointComparerResult : IComparer<DataPoint> {
            public int Compare (DataPoint x, DataPoint y) {
                return x.Result.CompareTo(y.Result);
            }
        }
        public class DataPointComparerVariable : IComparer<DataPoint> {

            int varIndex = 0;

            public DataPointComparerVariable (int varIndex) {
                this.varIndex = varIndex;
            }

            public int Compare (DataPoint x, DataPoint y) {
                return x.Data[varIndex].CompareTo(y.Data[varIndex]);
            }
        }

        public struct DataPoint : IComparable<DataPoint> {

            public CurrencyPair Pair;
            public long Timestamp;
            public double Result;
            public double[] Data;

            public DataPoint (CurrencyPair pair, long timestamp, double price, double[] data) {
                this.Pair = pair;
                this.Timestamp = timestamp;
                this.Result = price;
                this.Data = data;
            }
            public DataPoint (TickerChangedEventArgs ticker, double[] data) {
                this.Pair = ticker.CurrencyPair;
                this.Timestamp = ticker.Timestamp;
                this.Result = ticker.MarketData.PriceLast;
                this.Data = data;
            }

            public int CompareTo (DataPoint other) {
                return Timestamp.CompareTo(other.Timestamp);
            }

            // ----------------------------------------------

            public bool VerifyData () {
                if (Data == null) return false;
                for (int i = 0; i < Data.Length; i++) {
                    if (double.IsInfinity(Data[i]) || double.IsNaN(Data[i])) return false;
                }
                return true;
            }

            public override string ToString () {

                string line = Pair + "," + Timestamp;
                line += "," + Result.ToString("F8", System.Globalization.CultureInfo.InvariantCulture);

                for (int i = 0; i < Data.Length; i++) {
                    line += "," + Data[i].ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                }

                return line;
            }

            public static DataPoint Parse (string line) {
                string[] parts = line.Split(',');

                CurrencyPair pair = CurrencyPair.Parse(parts[0]);
                long timestamp = long.Parse(parts[1]);
                double price = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

                double[] data = new double[parts.Length - 3];
                for (int i = 3; i < parts.Length; i++) {
                    data[i - 3] = double.Parse(parts[i], System.Globalization.CultureInfo.InvariantCulture);
                }

                return new DataPoint(pair, timestamp, price, data);
            }
        }

        public static void PullAndGenerate (CurrencyPair pair) {

            long endTimestamp = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now) - (24 * 3600 * 0);
            long startTimestamp = endTimestamp - (24 * 3600 * 3);
            endTimestamp = startTimestamp;

            while (true) {
                try {

                    string filename = "data/pc_" + pair + ".data";
                    if (System.IO.File.Exists(filename)) {
                        System.IO.File.Delete(filename);
                    }

                    for (int i = 0; i < 3; i++) {

                        CLI.Manager.PrintNote("Generating day " + (i + 1) + "/3 for " + pair);

                        Data.Store.ClearTickerData();

                        startTimestamp = endTimestamp;
                        endTimestamp += 24 * 3600;

                        while (true) {
                            try {
                                if (Data.Store.PullTickerHistory(pair, startTimestamp - 14400, endTimestamp)) {
                                    Data.Store.SaveTradeData();

                                    System.Threading.Thread.Sleep(1500);
                                    Generate();

                                    break;
                                }
                                else break;
                            }
                            catch (Exception e) {
                                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                            }
                        }

                        System.Threading.Thread.Sleep(2500);
                    }

                    CalculateFuturePriceMovements(pair);

                    break;
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        public static void PullAndGenerate () {

            List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> markets =
                new List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>>(Data.Store.MarketData.ToArray());

            markets.Sort(new Utility.MarketDataComparerVolume());
            markets.Reverse();

            for (int i = 20; i < markets.Count; i++) {
                if (markets[i].Key.BaseCurrency == "BTC") {
                    PullAndGenerate(markets[i].Key);
                }
            }
        }

        public static void Generate () {

            // -----------------------------
            // load trade data
            // -----------------------------

            Utility.TSList<Utility.TSList<TickerChangedEventArgs>> fullTickerList = Data.Store.LoadTradeData(false);
            if (fullTickerList == null) throw new Exception("Data store loaded NULL tickers");

            KeyValuePair<CurrencyPair, List<TickerChangedEventArgs>>[] pairTickers =
                new KeyValuePair<CurrencyPair, List<TickerChangedEventArgs>>[fullTickerList.Count];

            for (int i = 0; i < pairTickers.Length; i++) {
                pairTickers[i] = new KeyValuePair<CurrencyPair, List<TickerChangedEventArgs>>
                    (fullTickerList[i].First().CurrencyPair, new List<TickerChangedEventArgs>());
            }

            // -----------------------------
            // setup strategies
            // -----------------------------

            Trading.Strategies.TrainingStrategy[] strategies = new Trading.Strategies.TrainingStrategy[fullTickerList.Count];

            for (int i = 0; i < fullTickerList.Count; i++) {
                CurrencyPair pair = fullTickerList[i].First().CurrencyPair;

                Data.Store.allowUpdatePairs.Add(pair);

                Trading.Strategies.TrainingStrategy strategy = new Trading.Strategies.TrainingStrategy(pair);
                strategy.GeneratePredictors();

                strategies[i] = strategy;
            }

            // -----------------------------
            // add initial data
            // -----------------------------

            for (int i = 0; i < fullTickerList.Count; i++) {
                long endTime = fullTickerList[i].First().Timestamp + 14400;

                Console.WriteLine("Adding initial data for " + fullTickerList[i].First().CurrencyPair);

                long lastTickerTimestamp = 0;
                for (int j = 0; j < fullTickerList[i].Count; j++) {
                    if (fullTickerList[i][j].Timestamp > endTime) break;
                    if (fullTickerList[i][j].Timestamp < lastTickerTimestamp + 5) continue;
                    lastTickerTimestamp = fullTickerList[i][j].Timestamp;

                    Data.Store.AddTickerData(fullTickerList[i][j]);

                    strategies[i].Recalculate();
                }
            }

            // -----------------------------
            // add the rest of data
            // -----------------------------

            for (int i = 0; i < fullTickerList.Count; i++) {
                long startTime = fullTickerList[i].First().Timestamp + 14400;

                Console.WriteLine("Adding full data for " + fullTickerList[i].First().CurrencyPair);

                string filePath = "data/pc_" + fullTickerList[i].First().CurrencyPair + ".data";

                // generate and add new data
                List<DataPoint> resultData = new List<DataPoint>();

                long lastTickerTimestamp = 0;
                for (int j = 0; j < fullTickerList[i].Count; j++) {
                    if (fullTickerList[i][j].Timestamp < startTime) continue;
                    if (fullTickerList[i][j].Timestamp < lastTickerTimestamp + 5) continue;
                    lastTickerTimestamp = fullTickerList[i][j].Timestamp;

                    Data.Store.AddTickerData(fullTickerList[i][j]);

                    double[] results = strategies[i].Recalculate();

                    resultData.Add(new DataPoint(fullTickerList[i][j], results));

                    if (j % 1000 == 0 && j > 0) {

                        List<string> lines = new List<string>();
                        for (int z = 0; z < resultData.Count; z++) {
                            lines.Add(resultData[z].ToString());
                        }

                        Utility.FileManager.SaveFileConcat(filePath, lines.ToArray());

                        resultData.Clear();

                        Console.WriteLine(fullTickerList[i][j].CurrencyPair + ": " + j + " / " + fullTickerList[i].Count);
                    }
                }

                strategies[i].Dispose();
                strategies[i] = null;
            }
        }

        public static void CalculateFuturePriceMovements (CurrencyPair pair) {

            // load precalculated data
            DataPoint[][] data = LoadPrecalculatedData();
            DataPoint[] fullData = null;
            for (int i = 0; i < data.Length; i++) {
                if (data[i].First().Pair == pair) {
                    fullData = data[i];
                    break;
                }
            }

            if (fullData == null) {
                Console.WriteLine("Data for pair " + pair + " not found! Pull and precalculate it first!");
                return;
            }

            // calculate future price movements
            Console.WriteLine("Calculating future price movements for " + pair);

            Data.Predictors.FuturePriceExtremes predictor = new Data.Predictors.FuturePriceExtremes(pair);
            predictor.Calculate(fullData);

            // write down data
            List<string> lines = new List<string>();
            for (int i = 0; i < fullData.Length; i++) {
                if (fullData[i].VerifyData()) lines.Add(fullData[i].ToString());
            }

            string filePath = "data/pc_" + pair + ".data";
            Utility.FileManager.SaveFile(filePath, lines.ToArray());

        }

        private static DataPoint[][] LoadPrecalculatedData () {

            string[] files = System.IO.Directory.GetFiles("data");
            if (files == null) return null;

            List<DataPoint[]> data = new List<DataPoint[]>();

            for (int i = 0; i < files.Length; i++) {
                string filename = files[i].Split('\\', '/').Last();

                if (filename.StartsWith("pc_")) {
                    Console.WriteLine("Loading file " + filename);

                    string[] lines = Utility.FileManager.ReadFile(files[i]);
                    DataPoint[] dataArray = new DataPoint[lines.Length];

                    Console.WriteLine("Parsing file " + filename + " (" + lines.Length + " data points)");
                    for (int j = 0; j < lines.Length; j++) {
                        dataArray[j] = DataPoint.Parse(lines[j]);
                    }

                    data.Add(dataArray);
                }
            }

            return data.ToArray();
        }

        private static void SaveData (CurrencyPair pair, long timestamp, double result, Optimizer[] optTimespan, Optimizer[] optTriggers) {

            List<string> lines = new List<string>();

            lines.Add(pair.ToString());
            lines.Add(timestamp.ToString());
            lines.Add(result.ToString("F8", System.Globalization.CultureInfo.InvariantCulture));

            for (int i = 0; i < optTimespan.Length; i++) {
                lines.Add(optTimespan[i].GetRawValue().ToString("F8", System.Globalization.CultureInfo.InvariantCulture));
            }
            for (int i = 0; i < optTriggers.Length; i++) {
                lines.Add(optTriggers[i].GetValue().ToString("F8", System.Globalization.CultureInfo.InvariantCulture));
            }

            Utility.FileManager.SaveFile("data/" + pair + "_optimization.data", lines.ToArray());

        }
    }
}

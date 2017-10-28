using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using Utility;

namespace PoloniexBot.Data.PatternMatching {
    public static class Manager {

        // ------------------------------------------

        const int Period = 900; // 5 min
        const int PeriodCount = 8; // 40m with 5m periods

        const string PatternsDirectory = "data";
        const string PatternsFilenamePrefix = "patterns";

        // ------------------------------------------

        public static List<KeyValuePair<CurrencyPair, List<Pattern>>> Repo;

        // ------------------------------------------

        public static void BuildPatternDatabase () {
            if (Repo == null) Repo = new List<KeyValuePair<CurrencyPair, List<Pattern>>>();
            
            TSList<TSList<TickerChangedEventArgs>> allTickers = Data.Store.LoadTradeData(false);
            if (allTickers == null) return;

            int minTime = Period * PeriodCount;

            for (int i = 0; i < allTickers.Count; i++) {
                if (allTickers[i].Count == 0) continue;

                CurrencyPair currPair = allTickers[i].First().CurrencyPair;
                long startTime = allTickers[i].First().Timestamp + minTime;
                int startIndex = 0;

                long lastTimestamp = 0;

                for (int j = 0; j < allTickers[i].Count; j++) {
                    long currTimestamp = allTickers[i][j].Timestamp;

                    if (currTimestamp > startTime) break;

                    lastTimestamp = currTimestamp;
                    startIndex = j;
                }

                int endIndex = allTickers[i].Count - 1;
                long endTime = allTickers[i][endIndex].Timestamp;

                for (int j = endIndex; j >= 0; j--) {
                    if (allTickers[i][j].Timestamp + Period < endTime) break;
                    endIndex = j;
                }

                List<Pattern> repoList = null;
                for (int j = 0; j < Repo.Count; j++) {
                    if (currPair == Repo[j].Key) {
                        repoList = Repo[j].Value;
                        break;
                    }
                }
                if (repoList == null) {
                    repoList = new List<Pattern>();
                    Repo.Add(new KeyValuePair<CurrencyPair, List<Pattern>>(currPair, repoList));
                }

                for (int j = startIndex; j < endIndex; j++) {
                    long currTimestamp = allTickers[i][j].Timestamp;

                    if (j % 1000 == 0) {
                        float progress = ((float)j / endIndex) * 100;
                        Console.WriteLine("Progress: " + i + "/" + allTickers.Count + " - " + progress.ToString("F2") + "%");
                    }

                    if (currTimestamp < lastTimestamp + 5) continue;
                    lastTimestamp = currTimestamp;

                    Pattern p = BuildPattern(allTickers[i].ToArray(), j, true);
                    repoList.Add(p);
                }

                SaveToFile();
            }
        }

        public static Pattern BuildPattern (TickerChangedEventArgs[] tickers, int index, bool includeFuture) {

            List<double> movements = new List<double>(GetPatternData(new List<TickerChangedEventArgs>(tickers), index, Period, PeriodCount));

            if (includeFuture) {
                double futureMove;
                GetFutureChange(new List<TickerChangedEventArgs>(tickers), index, Period, out futureMove);
                movements.Add(futureMove);
            }

            return new Pattern(movements.ToArray());
        }

        private static bool GetFutureChange (List<TickerChangedEventArgs> tickers, int startIndex, long timespan, out double value) {
            long endTimestamp = tickers[startIndex].Timestamp + timespan;
            value = 0;

            double startValue = tickers[startIndex].MarketData.PriceLast;
            double endValue = startValue;

            for (int i = 0; i < tickers.Count; i++) {
                if (i == tickers.Count - 1) return false;

                if (tickers[i].Timestamp > endTimestamp) break;
                endValue = tickers[i].MarketData.PriceLast;
            }

            value = ((endValue - startValue) / startValue) * 100;
            return true;
        }
        private static double[] GetPatternData (List<TickerChangedEventArgs> tickers, int endIndex, long timespans, int periods) {

            double currEndTime = tickers[endIndex].Timestamp;
            double currStartTime = currEndTime - timespans;

            int currStartIndex = endIndex;
            int currEndIndex = endIndex;

            List<double> changes = new List<double>();

            for (int i = 0; i < periods; i++) {

                double endValue = tickers[currEndIndex].MarketData.PriceLast;
                double startValue = endValue;

                for (int j = currEndIndex; j >= 0; j--) {
                    if (tickers[j].Timestamp < currStartTime) break;

                    startValue = tickers[j].MarketData.PriceLast;
                    currStartIndex = j;
                }

                changes.Add(((endValue - startValue) / startValue) * 100);

                currEndTime = currStartTime;
                currStartTime = currEndTime - timespans;

                currEndIndex = currStartIndex;
            }

            changes.Reverse();
            return changes.ToArray();
        }

        public static Pattern AnalyzePattern (Pattern p, CurrencyPair pair) {
            if (Repo == null) return null;
            // return the closest pattern to p

            // compare with every pattern in the repo

            List<KeyValuePair<Pattern, double>> data = new List<KeyValuePair<Pattern, double>>();

            List<Pattern> repoList = null;
            for (int j = 0; j < Repo.Count; j++) {
                if (pair == Repo[j].Key) {
                    repoList = Repo[j].Value;
                    break;
                }
            }
            if (repoList == null) return null;

            for (int i = 0; i < repoList.Count; i++) {
                double d = p.GetDistance(repoList[i]);
                data.Add(new KeyValuePair<Pattern, double>(repoList[i], d));
            }

            // sort based on distance (similarity)

            data.Sort(new Data.PatternMatching.Pattern.PatternComparer());

            return data.First().Key;
        }
        
        // ------------------------------------------
        // file save + load
        // ------------------------------------------

        public static void SaveToFile () {
            if (Repo == null) return;

            // save mapped patterns (cumulative, don't overwrite existing)

            for (int i = 0; i < Repo.Count; i++) {
                List<string> lines = new List<string>();
                for (int j = 0; j < Repo[i].Value.Count; j++) {
                    lines.Add(Repo[i].Value[j].ToString());
                }

                Utility.FileManager.SaveFile(PatternsDirectory + "/" + PatternsFilenamePrefix + "_" + Repo[i].Key + ".data", lines.ToArray());
            }
        }
        public static void LoadFromFile () {
            if (Repo == null) Repo = new List<KeyValuePair<CurrencyPair, List<Pattern>>>();
            Repo.Clear();

            string[] allFiles = System.IO.Directory.GetFiles(PatternsDirectory);
            for (int i = 0; i < allFiles.Length; i++) {
                allFiles[i] = allFiles[i].Replace("\\", "/");
                string cleaned = allFiles[i].Split('/').Last();

                if (cleaned.StartsWith(PatternsFilenamePrefix)) {

                    string currPairBase = cleaned.Split('_')[1];
                    string currPairQuote = cleaned.Split('_').Last().Split('.').First();

                    CurrencyPair pair = new CurrencyPair(currPairBase, currPairQuote);

                    string[] lines = Utility.FileManager.ReadFile(allFiles[i]);
                    if (lines != null) {
                        List<Pattern> patterns = new List<Pattern>();
                        for (int j = 0; j < lines.Length; j++) {
                            Pattern p = Pattern.Parse(lines[j]);
                            patterns.Add(p);
                        }
                        Repo.Add(new KeyValuePair<CurrencyPair, List<Pattern>>(pair, patterns));
                    }
                }
            }
        }
    }
}

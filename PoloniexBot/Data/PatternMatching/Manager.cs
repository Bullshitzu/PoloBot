using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.PatternMatching {
    public static class Manager {

        // ------------------------------------------

        const int Period = 300; // 5 min
        const int PeriodCount = 8; // 40m with 5m periods

        const string PatternsDirectory = "data";
        const string PatternsFilenamePrefix = "patterns_";

        // ------------------------------------------

        private static Dictionary<Trading.MarketAction, List<Pattern>> PatternRepo;

        // ------------------------------------------

        public static double AnalyzePattern (Pattern p, Trading.MarketAction type, int count = 10) {
            KeyValuePair<Pattern, double>[] patterns = ComparePattern(p, type);
            if (patterns == null) return double.MaxValue;

            double sum = 0;
            int cnt = 0;
            for (int i = 0; i < patterns.Length && i < count; i++) {
                sum += patterns[i].Value;
                cnt++;
            }

            return sum / cnt;
        }

        public static void MapPattern (TickerChangedEventArgs[] tickers, Trading.MarketAction action) {
            Pattern p = GeneratePattern(tickers);
            MapPattern(p, action);
        }
        public static void MapPattern (Pattern p, Trading.MarketAction action) {
            if (p == null) return;

            // add the pattern
            if (PatternRepo == null) PatternRepo = new Dictionary<Trading.MarketAction, List<Pattern>>();

            List<Pattern> lp;
            if (PatternRepo.TryGetValue(action, out lp)) {
                lp.Add(p);
            }
            else {
                lp = new List<Pattern>();
                lp.Add(p);
                PatternRepo.Add(action, lp);
            }
        }

        public static KeyValuePair<Pattern, double>[] ComparePattern (TickerChangedEventArgs[] tickers, Trading.MarketAction type) {
            Pattern p = GeneratePattern(tickers);
            return ComparePattern(p, type);
        }
        public static KeyValuePair<Pattern, double>[] ComparePattern (Pattern p, Trading.MarketAction type) {
            if (PatternRepo == null) return null;
            if (p == null) return null;

            List<Pattern> patterns;
            if (PatternRepo.TryGetValue(type, out patterns)) {
                List<KeyValuePair<Pattern, double>> lst = new List<KeyValuePair<Pattern, double>>();

                for (int i = 0; i < patterns.Count; i++) {
                    double dist = p.GetDistance(patterns[i]);
                    if (!double.IsNaN(dist)) lst.Add(new KeyValuePair<Pattern, double>(patterns[i], p.GetDistance(patterns[i])));
                }

                lst.Sort(new Pattern.PatternComparer());
                return lst.ToArray();
            }
            return null;
        }

        public static Pattern GeneratePattern (TickerChangedEventArgs[] tickers) {

            if (tickers == null || tickers.Length == 0) return null;

            // check if the tickers extend over the standard pattern timeframe
            long startTime = tickers.First().Timestamp;
            long endTime = tickers.Last().Timestamp;
            long breakTime = endTime - (Period * PeriodCount);

            // filter out the unnecessary tickers
            List<TickerChangedEventArgs> tempTickers = new List<TickerChangedEventArgs>();
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < breakTime) break;
                tempTickers.Add(tickers[i]);
            }
            tempTickers.Reverse();
            tickers = tempTickers.ToArray();

            startTime = tickers.First().Timestamp;
            endTime = tickers.Last().Timestamp;

            long fullTimeframe = endTime - startTime;

            // assemble the price movements
            double[] priceChanges = new double[PeriodCount];
            int startIndex = 0;

            for (int i = 0; i < PeriodCount; i++) {

                long currStartTime = ((fullTimeframe / PeriodCount) * i) + startTime;
                long currEndTime = currStartTime + Period;

                double startPrice = tickers[startIndex].MarketData.PriceLast;
                double endPrice = startPrice;

                for (int j = startIndex; j < tickers.Length; j++) {
                    TickerChangedEventArgs currTicker = tickers[j];

                    if (currTicker.Timestamp > currEndTime) break;

                    endPrice = currTicker.MarketData.PriceLast;
                    startIndex = j;
                }

                double priceChange = ((endPrice - startPrice) / startPrice) * 100;
                priceChanges[i] = priceChange;

            }

            return GeneratePattern(priceChanges);
        }
        public static Pattern GeneratePattern (double[] values) {
            if (values == null) return null;
            return new Pattern(values);
        }

        public static void SaveToFile () {
            if (PatternRepo == null) return;
            
            // save mapped patterns (cumulative, don't overwrite existing)

            KeyValuePair<Trading.MarketAction, List<Pattern>>[] patterns = PatternRepo.ToArray();
            if (patterns == null) return;

            for (int i = 0; i < patterns.Length; i++) {
                List<string> lines = new List<string>();
                for (int j = 0; j < patterns[i].Value.Count; j++) {
                    lines.Add(patterns[i].Value[j].ToString());
                }
                Utility.FileManager.SaveFile(PatternsDirectory + "/" + PatternsFilenamePrefix + patterns[i].Key.ToString(), lines.ToArray());
            }
        }
        public static void LoadFromFile () {
            if (PatternRepo == null) PatternRepo = new Dictionary<Trading.MarketAction, List<Pattern>>();
            PatternRepo.Clear();

            string buyFilename = PatternsDirectory + "/" + PatternsFilenamePrefix + Trading.MarketAction.Buy.ToString();
            string sellFilename = PatternsDirectory + "/" + PatternsFilenamePrefix + Trading.MarketAction.Sell.ToString();

            string[] lines = Utility.FileManager.ReadFile(buyFilename);
            if (lines != null) {
                List<Pattern> patterns = new List<Pattern>();
                for (int i = 0; i < lines.Length; i++) {
                    Pattern p = Pattern.Parse(lines[i]);
                    patterns.Add(p);
                }
                PatternRepo.Add(Trading.MarketAction.Buy, patterns);
            }

            lines = Utility.FileManager.ReadFile(sellFilename);
            if (lines != null) {
                List<Pattern> patterns = new List<Pattern>();
                for (int i = 0; i < lines.Length; i++) {
                    Pattern p = Pattern.Parse(lines[i]);
                    patterns.Add(p);
                }
                PatternRepo.Add(Trading.MarketAction.Sell, patterns);
            }
        }
    }
}

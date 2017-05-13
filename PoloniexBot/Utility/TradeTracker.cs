using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace Utility {
    public static class TradeTracker {

        const string DirectoryName = "Logs";
        const string Filename = "openPositions.data";

        static TradeTracker () {
            buyTrades = new List<TradeData>();
            sellTrades = new List<TradeData>();
            matches = new List<TradeMatch>();
        }

        public struct TradeData {
            public CurrencyPair pair;
            public double amountQuote;
            public double price;
            public long timestamp;
            public bool matched;

            public double openPrice;

            public TradeData (CurrencyPair pair, double amountQuote, double price, long timestamp) {
                this.pair = pair;
                this.amountQuote = amountQuote;
                this.price = price;
                this.openPrice = price;
                this.timestamp = timestamp;
                matched = false;
            }
            public TradeData (TradeData old, double newOpenPrice) {
                pair = old.pair;
                amountQuote = old.amountQuote;
                price = old.price;
                timestamp = old.timestamp;
                matched = old.matched;

                openPrice = newOpenPrice;
            }

            public static TradeData Parse (string[] lines) {
                if (lines == null || lines.Length != 4) throw new FormatException("Error Parsing TradeData!");

                CurrencyPair pair = CurrencyPair.Parse(lines[0]);
                long timestamp = long.Parse(lines[1]);
                double amountQuote = double.Parse(lines[2]);
                double price = double.Parse(lines[3]);

                return new TradeData(pair, amountQuote, price, timestamp);
            }
            public string[] Serialize () {
                string[] data = { pair.ToString(), timestamp.ToString(), amountQuote.ToString("F8"), price.ToString("F8") };
                return data;
            }
        }
        public struct TradeMatch {
            public TradeData buyTrade;
            public TradeData sellTrade;

            public double percentGain;
            public double netGainBtc;
            public double cumulativeNetGainBtc;

            public TradeMatch (TradeData buyTrade, TradeData sellTrade) {
                this.buyTrade = buyTrade;
                this.sellTrade = sellTrade;

                buyTrade.matched = true;
                sellTrade.matched = true;

                percentGain = ((sellTrade.price - buyTrade.price) / buyTrade.price) * 100;
                netGainBtc = (sellTrade.amountQuote * sellTrade.price) - (buyTrade.amountQuote * buyTrade.price);
                cumulativeNetGainBtc = 0;
            }
        }

        private static List<TradeData> buyTrades;
        private static List<TradeData> sellTrades;
        private static List<TradeMatch> matches;

        public static void ClearAll () {
            if (buyTrades != null) buyTrades.Clear();
            if (sellTrades != null) sellTrades.Clear();
            if (matches != null) matches.Clear();
        }

        public static void ReportBuy (CurrencyPair pair, double amountQuote, double price) {
            ReportBuy(pair, amountQuote, price, DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now));
        }
        public static void ReportBuy (CurrencyPair pair, double amountQuote, double price, long timestamp) {
            if (buyTrades == null) buyTrades = new List<TradeData>();

            TradeData tempData = new TradeData(pair, amountQuote, price, timestamp);
            buyTrades.Add(tempData);

            UpdateTradesGUI();

            SaveData();
        }

        public static void ReportSell (CurrencyPair pair, double amountQuote, double price) {
            ReportSell(pair, amountQuote, price, DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now));
        }
        public static void ReportSell (CurrencyPair pair, double amountQuote, double price, long timestamp) {
            if (sellTrades == null) sellTrades = new List<TradeData>();

            TradeData tempData = new TradeData(pair, amountQuote, price, timestamp);

            MatchTrades(tempData);
            CleanupOpenTrades();
            SaveData();
            UpdateTradesGUI();
        }

        private static void MatchTrades (TradeData sellTrade) {
            if (buyTrades == null || buyTrades.Count == 0) return;

            for (int i = buyTrades.Count-1; i >= 0; i--) {
                if (buyTrades[i].pair == sellTrade.pair) {
                    matches.Add(new TradeMatch(buyTrades[i], sellTrade));
                    buyTrades.RemoveAt(i);
                    break;
                }
            }
        }

        private static void SaveData () {
            if (buyTrades == null) return;

            List<string> lines = new List<string>();
            lines.Add(buyTrades.Count.ToString());
            for (int i = 0; i < buyTrades.Count; i++) {
                lines.AddRange(buyTrades[i].Serialize());
            }

            FileManager.SaveFile(DirectoryName + "/" + Filename, lines.ToArray());
        }
        public static void LoadData () {
            string[] lines = FileManager.ReadFile(DirectoryName + "/" + Filename);
            if(lines == null) return;

            if (buyTrades == null) buyTrades = new List<TradeData>();

            int cnt = int.Parse(lines[0]);
            for (int i = 0; i < cnt; i++) {
                string[] vars = new string[4];

                for (int j = 0; j < 4; j++) {
                    vars[j] = lines[i * 4 + 1 + j];
                }

                TradeData td = TradeData.Parse(vars);
                buyTrades.Add(td);

            }

            UpdateTradesGUI();
        }

        private static void CleanupOpenTrades () {
            if (buyTrades != null) buyTrades.RemoveAll(m => m.matched == true);
            if (sellTrades != null) sellTrades.RemoveAll(m => m.matched == true);
        }

        private static void UpdateTradesGUI () {
            PoloniexBot.Windows.GUIManager.tradeHistoryWindow.tradeHistoryScreen.UpdateTrades(buyTrades.ToArray(), sellTrades.ToArray(), matches.ToArray());
        }

        public static bool GetOpenPosition (CurrencyPair pair, ref double value) {
            if (buyTrades == null) return false;

            for (int i = 0; i < buyTrades.Count; i++) {
                if (buyTrades[i].pair == pair) {
                    value = buyTrades[i].price;
                    return true;
                }
            }

            return false;
        }
        public static void UpdateOpenPosition (CurrencyPair pair, double price) {
            if (buyTrades == null) return;

            for (int i = 0; i < buyTrades.Count; i++) {
                if (buyTrades[i].pair == pair) {
                    buyTrades[i] = new TradeData(buyTrades[i], price);
                    UpdateTradesGUI();
                    return;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace Utility {
    public static class TradeTracker {

        const string DirectoryName = "Logs";
        const string OpenPositionsFilename = "openPositions.data";

        static TradeTracker () {
            Trades = new List<TradeData>();
            DoneTrades = new List<TradeData>();
        }

        public struct TradeData {
            public CurrencyPair pair;
            public double buyAmountQuote;
            public double buyPrice;
            public long buyTimestamp;

            public bool sold;
            public double openPrice;

            public double sellAmountQuote;
            public double sellPrice;
            public long sellTimestamp;

            public double percentGain;
            public double netGainBtc;
            public double cumulativeNetGainBtc;

            public TradeData (CurrencyPair pair, double amountQuote, double price, long timestamp) {
                
                // Buy constructor
                
                this.pair = pair;
                this.buyAmountQuote = amountQuote;
                this.buyPrice = price;
                this.buyTimestamp = timestamp;

                this.openPrice = price;
                sold = false;

                sellAmountQuote = 0;
                sellPrice = 0;
                sellTimestamp = 0;

                percentGain = 0;
                netGainBtc = 0;
                cumulativeNetGainBtc = 0;
            }
            public TradeData (TradeData old, double newOpenPrice) {

                // Update constructor

                this.pair = old.pair;
                this.buyAmountQuote = old.buyAmountQuote;
                this.buyPrice = old.buyPrice;
                this.buyTimestamp = old.buyTimestamp;

                openPrice = newOpenPrice;
                sold = false;

                sellAmountQuote = 0;
                sellPrice = 0;
                sellTimestamp = 0;

                percentGain = 0;
                netGainBtc = 0;
                cumulativeNetGainBtc = 0;
            }
            public TradeData (TradeData old, double amountQuote, double price, long timestamp) {

                // Sold constructor

                this.pair = old.pair;
                this.buyAmountQuote = old.buyAmountQuote;
                this.buyPrice = old.buyPrice;
                this.buyTimestamp = old.buyTimestamp;

                openPrice = price;
                sold = true;

                sellAmountQuote = amountQuote;
                sellPrice = price;
                sellTimestamp = timestamp;

                percentGain = ((sellPrice - buyPrice) / buyPrice) * 100;
                netGainBtc = (sellAmountQuote * sellPrice) - (buyAmountQuote * buyPrice);
                cumulativeNetGainBtc = 0;
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
                string[] data = { pair.ToString(), buyTimestamp.ToString(), buyAmountQuote.ToString("F8"), buyPrice.ToString("F8") };
                return data;
            }
        }
        
        private static List<TradeData> Trades;
        private static List<TradeData> DoneTrades;

        public static void ClearAll () {
            if (Trades != null) Trades.Clear();
            if (DoneTrades != null) DoneTrades.Clear();
            UpdateTradesGUI();
        }

        public static void ReportBuy (CurrencyPair pair, double amountQuote, double price) {
            ReportBuy(pair, amountQuote, price, DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now));
        }
        public static void ReportBuy (CurrencyPair pair, double amountQuote, double price, long timestamp) {
            if (Trades == null) Trades = new List<TradeData>();

            TradeData tempData = new TradeData(pair, amountQuote, price, timestamp);
            Trades.Add(tempData);

            UpdateTradesGUI();

            SaveData();
        }

        public static void ReportSell (CurrencyPair pair, double amountQuote, double price) {
            ReportSell(pair, amountQuote, price, DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now));
        }
        public static void ReportSell (CurrencyPair pair, double amountQuote, double price, long timestamp) {
            if (Trades == null) return;
            if (DoneTrades == null) DoneTrades = new List<TradeData>();

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    DoneTrades.Add(new TradeData(Trades[i], amountQuote, price, timestamp));
                    Trades.RemoveAt(i);
                    UpdateTradesGUI();
                    SaveData();
                    return;
                }
            }
        }

        private static void SaveData () {
            if (Trades == null) return;
            if (PoloniexBot.ClientManager.Simulate) return;

            List<string> lines = new List<string>();
            lines.Add(Trades.Count.ToString());
            for (int i = 0; i < Trades.Count; i++) {
                lines.AddRange(Trades[i].Serialize());
            }

            FileManager.SaveFile(DirectoryName + "/" + OpenPositionsFilename, lines.ToArray());
        }
        public static void LoadData () {
            if (PoloniexBot.ClientManager.Simulate) return;

            string[] lines = FileManager.ReadFile(DirectoryName + "/" + OpenPositionsFilename);
            if(lines == null) return;

            if (Trades == null) Trades = new List<TradeData>();

            int cnt = int.Parse(lines[0]);
            for (int i = 0; i < cnt; i++) {
                string[] vars = new string[4];

                for (int j = 0; j < 4; j++) {
                    vars[j] = lines[i * 4 + 1 + j];
                }

                TradeData td = TradeData.Parse(vars);
                Trades.Add(td);

            }

            UpdateTradesGUI();
        }

        private static void CleanupOpenTrades () {
            if (Trades != null) while (Trades.Count > 12) Trades.RemoveAt(0);
        }

        private static void UpdateTradesGUI () {
            PoloniexBot.Windows.GUIManager.tradeHistoryWindow.tradeHistoryScreen.UpdateTrades(Trades.ToArray(), DoneTrades.ToArray());
        }

        public static double GetOpenPosition (CurrencyPair pair) {
            if (Trades == null) return 0;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    if (Trades[i].sold) return 0;
                    return Trades[i].buyPrice;
                }
            }

            return 0;
        }
        public static long GetOpenPositionBuyTime (CurrencyPair pair) {
            if (Trades == null) return 0;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    if (Trades[i].sold) return 0;
                    return Trades[i].buyTimestamp;
                }
            }

            return 0;
        }
        public static void UpdateOpenPosition (CurrencyPair pair, double price) {
            if (Trades == null) return;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    Trades[i] = new TradeData(Trades[i], price);
                    UpdateTradesGUI();
                    return;
                }
            }
        }
    }
}

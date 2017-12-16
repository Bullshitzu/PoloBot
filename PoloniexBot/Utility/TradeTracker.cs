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
        const string ClosedPositionsFilename = "closedPositions.data";

        static TradeTracker () {
            Trades = new TSList<TradeData>();
            DoneTrades = new TSList<TradeData>();
        }

        public class TradeData {
            public CurrencyPair pair;
            public double buyAmountQuote;
            public double buyPrice;
            public long buyTimestamp;

            public ulong orderID;
            public double orderPrice;

            public bool sold;
            public double openPrice;

            public double sellAmountQuote;
            public double sellPrice;
            public long sellTimestamp;

            public double percentGain;
            public double netGainBtc;
            public double cumulativeNetGainBtc;

            public double stopLossPercent;

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

                orderID = 0;
                orderPrice = 0;

                stopLossPercent = double.MinValue;
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

                percentGain = ((openPrice - buyPrice) / buyPrice) * 100;
                netGainBtc = 0;
                cumulativeNetGainBtc = 0;

                orderID = 0;
                orderPrice = 0;

                stopLossPercent = old.stopLossPercent;
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

                orderID = 0;
                orderPrice = 0;

                stopLossPercent = old.stopLossPercent;
            }

            public static TradeData Parse (string[] lines) {
                if (lines == null || lines.Length != 7) throw new FormatException("Error Parsing TradeData!");

                CurrencyPair pair = CurrencyPair.Parse(lines[0]);
                long buyTimestamp = long.Parse(lines[1]);
                double buyAmountQuote = double.Parse(lines[2]);
                double buyPrice = double.Parse(lines[3]);

                long sellTimestamp = long.Parse(lines[4]);
                double sellAmountQuote = double.Parse(lines[5]);
                double sellPrice = double.Parse(lines[6]);

                TradeData temp = new TradeData(pair, buyAmountQuote, buyPrice, buyTimestamp);

                if (sellTimestamp > 0) {
                    temp.sellTimestamp = sellTimestamp;
                    temp.sellAmountQuote = sellAmountQuote;
                    temp.sellPrice = sellPrice;

                    temp.percentGain = ((sellPrice - buyPrice) / buyPrice) * 100;
                    temp.netGainBtc = (sellAmountQuote * sellPrice) - (buyAmountQuote * buyPrice);
                }
                return temp;
            }
            public string[] Serialize () {
                string[] data = { 
                    pair.ToString(), 
                    buyTimestamp.ToString(), 
                    buyAmountQuote.ToString("F8"), 
                    buyPrice.ToString("F8"),
                    sellTimestamp.ToString(),
                    sellAmountQuote.ToString("F8"),
                    sellPrice.ToString("F8") };
                return data;
            }
        }
        
        private static TSList<TradeData> Trades;
        private static TSList<TradeData> DoneTrades;

        public static void ClearAll () {
            if (Trades != null) Trades.Clear();
            if (DoneTrades != null) DoneTrades.Clear();
            UpdateTradesGUI();
        }

        public static void ReportBuy (CurrencyPair pair, double amountQuote, double price) {
            ReportBuy(pair, amountQuote, price, DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now));
        }
        public static void ReportBuy (CurrencyPair pair, double amountQuote, double price, long timestamp) {
            if (Trades == null) Trades = new TSList<TradeData>();

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
            if (DoneTrades == null) DoneTrades = new TSList<TradeData>();

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

            List<string> linesOpen = SerializeTrades(Trades);
            List<string> linesClosed = SerializeTrades(DoneTrades);

            FileManager.SaveFile(DirectoryName + "/" + OpenPositionsFilename, linesOpen.ToArray());
            FileManager.SaveFile(DirectoryName + "/" + ClosedPositionsFilename, linesClosed.ToArray());
        }
        private static List<string> SerializeTrades (TSList<TradeData> trades) {
            if (trades == null) return new List<string>();

            List<string> lines = new List<string>();

            List<TradeData> filteredTrades = new List<TradeData>(trades.ToArray());
            while (filteredTrades.Count > 12) filteredTrades.RemoveAt(0);

            lines.Add(filteredTrades.Count.ToString());
            for (int i = 0; i < filteredTrades.Count; i++) {
                lines.AddRange(filteredTrades[i].Serialize());
            }

            return lines;
        }

        public static void LoadData () {
            if (PoloniexBot.ClientManager.Simulate) return;

            string[] lines;

            lines = FileManager.ReadFile(DirectoryName + "/" + OpenPositionsFilename);
            if (lines != null) Trades = DeserializeTrades(lines);

            lines = FileManager.ReadFile(DirectoryName + "/" + ClosedPositionsFilename);
            if (lines != null) {
                TSList<TradeData> tempClosedTrades = DeserializeTrades(lines);

                for (int i = 0; i < tempClosedTrades.Count; i++) {
                    tempClosedTrades[i] = new TradeData(
                        tempClosedTrades[i], 
                        tempClosedTrades[i].sellAmountQuote, 
                        tempClosedTrades[i].sellPrice, 
                        tempClosedTrades[i].sellTimestamp);
                }

                DoneTrades = tempClosedTrades;
            }

            UpdateTradesGUI();
        }
        private static TSList<TradeData> DeserializeTrades (string[] lines) {
            if (lines == null) return null;

            TSList<TradeData> tempTrades = new TSList<TradeData>();

            int cnt = int.Parse(lines[0]);
            for (int i = 0; i < cnt; i++) {
                string[] vars = new string[7];

                for (int j = 0; j < 7; j++) {
                    vars[j] = lines[i * 7 + 1 + j];
                }

                TradeData td = TradeData.Parse(vars);
                tempTrades.Add(td);

            }

            return tempTrades;
        }

        private static void CleanupOpenTrades () {
            if (Trades != null) while (Trades.Count > 12) Trades.RemoveAt(0);
        }

        private static void UpdateTradesGUI () {
            PoloniexBot.GUI.GUIManager.UpdateTradeHistory(Trades, DoneTrades);
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

        public static void UpdateStopLoss (CurrencyPair pair, double stopLoss) {
            if (Trades == null) return;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    Trades[i].stopLossPercent = stopLoss;
                    UpdateTradesGUI();
                    return;
                }
            }
        }

        internal static ulong GetOrderID (CurrencyPair pair) {
            if (Trades == null) return 0;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    return Trades[i].orderID;
                }
            }

            return 0;
        }
        internal static double GetOrderPrice (CurrencyPair pair) {
            if (Trades == null) return 0;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    return Trades[i].orderPrice;
                }
            }

            return 0;
        }

        internal static void SetOrderData (CurrencyPair pair, ulong id, double price) {
            if (Trades == null) return;

            for (int i = 0; i < Trades.Count; i++) {
                if (Trades[i].pair == pair) {
                    Trades[i].orderID = id;
                    Trades[i].orderPrice = price;
                    return;
                }
            }
        }
    }
}

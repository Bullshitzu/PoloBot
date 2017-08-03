using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PoloniexAPI;
using PoloniexAPI.MarketTools;
using Utility;

namespace PoloniexBot {
    namespace Data {
        public static class Store {

            public const int TickerStoreTime = 21600; // 6 hours

            public static void ClearAllData () {
                if (marketData != null) marketData.Clear();
                if (allowUpdatePairs != null) allowUpdatePairs.Clear();
                if (tickerData != null) tickerData.Clear();
            }
            public static void ClearTickerData () {
                if (tickerData != null) tickerData.Clear();
            }

            public static bool AllowTickerUpdate = true;

            // Market Data
            private static IDictionary<CurrencyPair, IMarketData> marketData;
            public static IDictionary<CurrencyPair, IMarketData> MarketData {
                get {
                    lock (marketData) {
                        return marketData;
                    }
                }
                set {
                    if (marketData == null) marketData = value;
                    else {
                        lock (marketData) {
                            marketData = value;
                        }
                    }
                }
            }

            // Ticker Data
            public static List<CurrencyPair> allowUpdatePairs;
            private static TSList<TSList<TickerChangedEventArgs>> tickerData;
            
            public static TickerChangedEventArgs[] GetTickerData (CurrencyPair currencyPair) {
                if (tickerData == null) return null;
                lock (tickerData) {
                    for (int i = 0; i < tickerData.Count; i++) {
                        if (tickerData[i] == null) continue;
                        if (tickerData[i].Count == 0) continue;
                        if (tickerData[i][0].CurrencyPair == currencyPair) return tickerData[i].ToArray();
                    }
                }
                return null;
            }
            public static TickerChangedEventArgs GetLastTicker (CurrencyPair currencyPair) {
                TickerChangedEventArgs[] tickers = GetTickerData(currencyPair);
                if (tickers == null) return null;
                return tickers.Last();
            }

            public static void AddTickerData (TickerChangedEventArgs ticker) {
                if (!AllowTickerUpdate) return;

                if (allowUpdatePairs == null) return;
                if (!allowUpdatePairs.Contains(ticker.CurrencyPair)) return;
                int deleteTime = (int)DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now) - TickerStoreTime;
                lock (tickerData) {
                    for (int i = 0; i < tickerData.Count; i++) {
                        if (tickerData[i] == null) continue;
                        if (tickerData[i].Count == 0) continue;
                        if (tickerData[i][0].CurrencyPair == ticker.CurrencyPair) {
                            // while (tickerData[i][0].Timestamp < deleteTime) tickerData[i].RemoveAt(0);
                            tickerData[i].Add(ticker);
                            Trading.Manager.NotifyTickerUpdate(ticker.CurrencyPair);
                            return;
                        }
                    }
                    TSList<TickerChangedEventArgs> list = new TSList<TickerChangedEventArgs>();
                    list.Add(ticker);
                    tickerData.Add(list);
                    Trading.Manager.NotifyTickerUpdate(ticker.CurrencyPair);
                }
            }
            public static CurrencyPair[] GetAvailableTickerPairs () {
                if (tickerData == null) return new CurrencyPair[0];
                List<CurrencyPair> currencies = new List<CurrencyPair>();
                for (int i = 0; i < tickerData.Count; i++) {
                    if (tickerData[i] == null) continue;
                    if (tickerData[i].Count == 0) continue;
                    currencies.Add(tickerData[i].First().CurrencyPair);
                }
                return currencies.ToArray();
            }
            public static bool CheckPairExists (CurrencyPair pair) {
                IMarketData whatevs;
                return marketData.TryGetValue(pair, out whatevs);
            }

            public static void PullTickerHistoryRecent (CurrencyPair pair) {
                if (!AllowTickerUpdate) return;

                if (tickerData == null) tickerData = new TSList<TSList<TickerChangedEventArgs>>();
                allowUpdatePairs = new List<CurrencyPair>();

                List<PoloniexAPI.MarketTools.ITrade> trades = WebApiCustom.GetTrades(pair);

                if (trades != null) {
                    
                    TickerChangedEventArgs[] fakeTickers = new TickerChangedEventArgs[trades.Count];
                    
                    for (int i = 0; i < trades.Count; i++) {
                        MarketData md = new MarketData(trades[i].PricePerCoin);
                        TickerChangedEventArgs ticker = new TickerChangedEventArgs(pair, md);
                        fakeTickers[i] = ticker;
                    }
                    
                    TSList<TickerChangedEventArgs> tickerList = new TSList<TickerChangedEventArgs>(fakeTickers);
                    tickerList.Sort();

                    tickerData.Add(tickerList);
                    allowUpdatePairs.Add(pair);
                }
            }
            public static void PullTickerHistory (CurrencyPair pair) {
                PullTickerHistory(pair, 6);
            }
            public static void PullTickerHistory (CurrencyPair pair, int hours) {
                if (!AllowTickerUpdate) return;

                if (tickerData == null) tickerData = new TSList<TSList<TickerChangedEventArgs>>();
                if (allowUpdatePairs == null) allowUpdatePairs = new List<CurrencyPair>();

                // pull last trades

                long currTime = DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
                long startTime = currTime - (hours * 3600);

                List<PoloniexAPI.MarketTools.ITrade> trades = new List<ITrade>();

                while (true) {
                    try {
                        List<PoloniexAPI.MarketTools.ITrade> temp = WebApiCustom.GetTrades(pair, (int)startTime, (int)currTime);
                        if (temp == null) throw new Exception("WebApiCustom returned NULL tickers");

                        Console.WriteLine("Pulled " + temp.Count + " tickers!");

                        trades.AddRange(temp);

                        if (temp.Count < 300) break;

                        currTime = Utility.DateTimeHelper.DateTimeToUnixTimestamp(temp.Last().Time);

                        ThreadManager.ReportAlive("Data.Store");
                        Thread.Sleep(1000);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message + "\n" + e.StackTrace);
                        Thread.Sleep(3000);
                    }
                }

                // convert into fake tickers

                TickerChangedEventArgs[] fakeTickers = new TickerChangedEventArgs[trades.Count];
                for (int j = 0; j < trades.Count; j++) {

                    MarketData md = new MarketData(trades[j].PricePerCoin);

                    TickerChangedEventArgs ticker = new TickerChangedEventArgs(pair, md);
                    ticker.Timestamp = DateTimeHelper.DateTimeToUnixTimestamp(trades[j].Time);

                    fakeTickers[j] = ticker;
                }

                // dump them into ticker data 

                TSList<TickerChangedEventArgs> tickerList = new TSList<TickerChangedEventArgs>(fakeTickers);
                tickerList.Sort();

                tickerData.Add(tickerList);
                allowUpdatePairs.Add(pair);
            }
            public static void PullTickerHistory (CurrencyPair pair, long startTimestamp, long endTimestamp) {
                if (!AllowTickerUpdate) return;

                if (tickerData == null) tickerData = new TSList<TSList<TickerChangedEventArgs>>();
                if (allowUpdatePairs == null) allowUpdatePairs = new List<CurrencyPair>();

                // pull last trades

                List<PoloniexAPI.MarketTools.ITrade> trades = new List<ITrade>();

                while (true) {
                    try {
                        List<PoloniexAPI.MarketTools.ITrade> temp = WebApiCustom.GetTrades(pair, (int)startTimestamp, (int)endTimestamp);
                        trades.AddRange(temp);

                        if (temp.Count < 300) break;

                        endTimestamp = Utility.DateTimeHelper.DateTimeToUnixTimestamp(temp.Last().Time);

                        ThreadManager.ReportAlive("Data.Store");
                        Thread.Sleep(1000);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message + "\n" + e.StackTrace);
                        Thread.Sleep(1000);
                    }
                }

                // convert into fake tickers

                TickerChangedEventArgs[] fakeTickers = new TickerChangedEventArgs[trades.Count];
                for (int j = 0; j < trades.Count; j++) {

                    MarketData md = new MarketData(trades[j].PricePerCoin);

                    TickerChangedEventArgs ticker = new TickerChangedEventArgs(pair, md);
                    ticker.Timestamp = DateTimeHelper.DateTimeToUnixTimestamp(trades[j].Time);

                    fakeTickers[j] = ticker;
                }

                // dump them into ticker data 

                TSList<TickerChangedEventArgs> tickerList = new TSList<TickerChangedEventArgs>(fakeTickers);
                tickerList.Sort();

                tickerData.Add(tickerList);
                allowUpdatePairs.Add(pair);
            }

            // Order Data
            public static Trading.OrderLive[] PullOrderHistory (CurrencyPair pair) {
                IOrderBook orderBook = ClientManager.client.Markets.GetOpenOrdersAsync(pair).Result;
                if (orderBook == null) return null;

                List<Trading.OrderLive> list = new List<Trading.OrderLive>();

                if (orderBook.BuyOrders != null) {
                    for (int i = 0; i < orderBook.BuyOrders.Count; i++) {
                        Trading.OrderLive order = new Trading.OrderLive(
                            Trading.OrderLiveType.Modify,
                            Trading.MarketAction.Buy,
                            orderBook.BuyOrders[i].AmountBase,
                            orderBook.BuyOrders[i].PricePerCoin);
                        list.Add(order);
                    }
                }
                if (orderBook.SellOrders != null) {
                    for (int i = 0; i < orderBook.SellOrders.Count; i++) {
                        Trading.OrderLive order = new Trading.OrderLive(
                            Trading.OrderLiveType.Modify,
                            Trading.MarketAction.Sell,
                            orderBook.SellOrders[i].AmountBase,
                            orderBook.SellOrders[i].PricePerCoin);
                        list.Add(order);
                    }
                }

                return list.ToArray();
            }

            // Trade Data
            public static IList<ITrade> GetTradeHistory (CurrencyPair pair) {
                return ClientManager.client.Markets.GetTradesAsync(pair).Result;
            }

            // File Managment
            public static void SaveTradeData () {

                List<TickerChangedEventArgs> allTickers = new List<TickerChangedEventArgs>();
                for (int i = 0; i < tickerData.Count; i++) {
                    for (int j = 0; j < tickerData[i].Count; j++) {
                        TickerChangedEventArgs currTicker = tickerData[i][j];
                        allTickers.Add(currTicker);
                    }
                }

                allTickers.Sort();

                List<string> lines = new List<string>();
                for (int i = 0; i < allTickers.Count; i++) {
                    TickerChangedEventArgs currTicker = allTickers[i];
                    string currLine = string.Format("{0}:{1}:{2}:{3}",
                        currTicker.CurrencyPair,
                        currTicker.Timestamp,
                        currTicker.MarketData.PriceLast.ToString("F8"),
                        currTicker.MarketData.Volume24HourBase.ToString("F8"));
                    lines.Add(currLine);
                }

                CLI.Manager.PrintLog("Saving ticker data to file (" + lines.Count + " tickers)");
                Utility.FileManager.SaveFile("data/ticker data", lines.ToArray());
                CLI.Manager.PrintLog("Done!");
            }
            public static TSList<TSList<TickerChangedEventArgs>> LoadTradeData (bool addTickers = true) {

                CLI.Manager.PrintLog("Clearing trade pairs");
                Trading.Manager.ClearAllPairs();

                CLI.Manager.PrintLog("Clearing current ticker data");
                if (allowUpdatePairs != null) allowUpdatePairs.Clear();
                if (tickerData == null) tickerData = new TSList<TSList<TickerChangedEventArgs>>();
                tickerData.Clear();

                CLI.Manager.PrintLog("Loading ticker data from file");
                string[] lines = Utility.FileManager.ReadFile("data/ticker data");
                if (lines == null || lines.Length == 0) throw new Exception("Failed reading file - no lines returned");

                TSList<TSList<TickerChangedEventArgs>> tickerStoreReference;
                if (addTickers) tickerStoreReference = tickerData;
                else tickerStoreReference = new TSList<TSList<TickerChangedEventArgs>>();

                for (int i = 0; i < lines.Length; i++) {
                    string[] parts = lines[i].Split(':');

                    CurrencyPair currPair = CurrencyPair.Parse(parts[0]);
                    long currTimestamp = long.Parse(parts[1]);
                    double currPrice = double.Parse(parts[2]);
                    double volume24Base = double.Parse(parts[3]);

                    TickerChangedEventArgs currTicker = new TickerChangedEventArgs(currPair, new MarketData(currPrice));
                    currTicker.Timestamp = currTimestamp;
                    currTicker.MarketData.Volume24HourBase = volume24Base;

                    if (allowUpdatePairs == null) allowUpdatePairs = new List<CurrencyPair>();
                    if (!allowUpdatePairs.Contains(currPair)) allowUpdatePairs.Add(currPair);

                    // add to list
                    
                    bool added = false;
                    for (int j = 0; j < tickerStoreReference.Count; j++) {
                        if (tickerStoreReference[j][0].CurrencyPair == currPair) {
                            tickerStoreReference[j].Add(currTicker);
                            added = true;
                            break;
                        }
                    }

                    if (!added) {
                        tickerStoreReference.Add(new TSList<TickerChangedEventArgs>());
                        tickerStoreReference.Last().Add(currTicker);
                    }
                }

                CLI.Manager.PrintLog("Loading complete (" + lines.Length + " tickers)");

                return tickerStoreReference;
            }
        }
    }
}

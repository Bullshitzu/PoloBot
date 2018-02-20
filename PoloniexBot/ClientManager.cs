using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PoloniexAPI;
using Utility;

namespace PoloniexBot {
    public static class ClientManager {

        static string keysFilename = "settings/APIKeys.file";
        public static PoloniexClient client;

        public static bool Simulate = true;
        public static bool Training = true;

        static string[] LoadApiKey () {
            return FileManager.ReadFile(keysFilename);
        }

        public static void Shutdown () {
            CLI.Manager.PrintLog("Global Shutdown Initiated");
            ThreadManager.KillNetwork();
            CLI.Manager.PrintLog("Clearing All Trade Data");
            Data.Store.ClearAllData();
            client = null;
        }

        public static void Reboot () {

            CLI.Manager.PrintLog("Booting Up");

            // API Keys
            CLI.Manager.PrintNote("Loading API Keys");
            string[] apiKey = LoadApiKey();
            if (apiKey == null || apiKey.Length != 2) {
                ErrorLog.ReportError("Failed reading API key! File damaged or missing?");
                return;
            }

            if (Simulate) {
                CLI.Manager.PrintNote("Initializing Simulated Client");
                client = new PoloniexClient(apiKey[0], apiKey[1], true);

                if (Training) GUI.GUIManager.SetEnvironment(GUI.GUIManager.Environment.Development);
                else GUI.GUIManager.SetEnvironment(GUI.GUIManager.Environment.Simulation);
            }
            else {
                CLI.Manager.PrintNote("Initializing Live Client");
                client = new PoloniexClient(apiKey[0], apiKey[1], false);

                GUI.GUIManager.SetEnvironment(GUI.GUIManager.Environment.Live);
            }

            // Clock
            CLI.Manager.PrintNote("Calculating Clock Offset");
            Utility.DateTimeHelper.RecalculateClockOffset();

            Thread.Sleep(1000);

            // Market Summary

            while (true) {
                try {
                    CLI.Manager.PrintNote("Pulling Market Summary");
                    RefreshMarketData();

                    Thread.Sleep(1000);
                    break;
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(5000);
                }
            }

            // Training

            if (Training) {
                ThreadManager.Register(() => {

                    // Data.VariableAnalysis.AnalyzeAllPairs();
                    
                    long endTimestamp = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now) - (24 * 3600 * 0);
                    long startTimestamp = endTimestamp - (24 * 3600 * 1);
                    /*
                    List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> allPairs =
                        new List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>>(Data.Store.MarketData.ToArray());

                    allPairs.Sort(new Utility.MarketDataComparerVolume());
                    allPairs.Reverse();

                    int added = 0;
                    // Data.Store.PullTickerHistory(new CurrencyPair("USDT", "BTC"), startTimestamp, endTimestamp);
                    
                    for (int i = 0; i < allPairs.Count; i++) {
                        if (allPairs[i].Key.BaseCurrency == "BTC") {

                            if (Data.Store.GetLastTicker(allPairs[i].Key).MarketData.PriceLast < 0.00001) continue;

                            try {

                                Console.WriteLine("Pulling " + allPairs[i]);
                                Data.Store.PullTickerHistory(allPairs[i].Key, startTimestamp, endTimestamp);
                                added++;
                                
                            }
                            catch (Exception e) {
                                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                            }

                            if (added >= 20) break;

                            Thread.Sleep(2000);
                        }

                    }
                    
                    /*
                    */

                    PullMostVolatilePairs(5, 21600, startTimestamp, endTimestamp);

                    Data.Store.SaveTradeData();
                    
                    Simulation.SimulateAll();

                }, "Data Pull", true);
            }
            else {

                // Ticker Feed
                /*
                while (true) {
                    try {
                        CLI.Manager.PrintNote("Subscribing To Ticker Feed");
                        client.Live.Start();
                        client.Live.SubscribeToTickerAsync();
                        client.Live.OnTickerChanged += new EventHandler<TickerChangedEventArgs>(Windows.GUIManager.tickerFeedWindow.RecieveMessage);
                        break;
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                        Thread.Sleep(5000);
                    }
                }
                */
                Thread.Sleep(1000);

                Data.PatternMatching.Manager.LoadFromFile();

                Trading.Manager.Start();
                ThreadManager.Register(Trading.Manager.RefreshTradePairs, "TP Refresh", true);
            }
        }

        // ---------------------------------------------

        private static void PullMostVolatilePairs (int number, long volatilityMeasurePeriod, long startTimestamp, long endTimestamp) {

            List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> allPairs =
                        new List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>>(Data.Store.MarketData.ToArray());

            for (int i = 0; i < allPairs.Count; i++) {
                if (allPairs[i].Key.BaseCurrency != "BTC") {
                    allPairs.RemoveAt(i);
                    i--;
                }
            }

            allPairs.Sort(new Utility.MarketDataComparerVolume());
            allPairs.Reverse();

            List<KeyValuePair<CurrencyPair, double>> pairVolatilites = new List<KeyValuePair<CurrencyPair, double>>();

            for (int i = 0; i < allPairs.Count && i < 30; i++) {

                if (allPairs[i].Value.PriceLast < 0.00001) continue;

                try {
                    Console.WriteLine("Pulling " + allPairs[i].Key + " for volatility analysis (" + i + "/30)");
                    Data.Store.PullTickerHistory(allPairs[i].Key, endTimestamp - volatilityMeasurePeriod, endTimestamp);

                    double vol = Trading.Manager.CalculateVolatility(allPairs[i].Key);
                    pairVolatilites.Add(new KeyValuePair<CurrencyPair, double>(allPairs[i].Key, vol));
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }

                Thread.Sleep(2000);

            }

            pairVolatilites.Sort(new Utility.MarketDataComparerTrend());
            pairVolatilites.Reverse();

            Data.Store.ClearTickerData();

            for (int i = 0; i < number; i++) {
                if (i >= pairVolatilites.Count) break;

                try {
                    Console.WriteLine("Pulling " + pairVolatilites[i]);
                    Data.Store.PullTickerHistory(pairVolatilites[i].Key, startTimestamp, endTimestamp);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }

                Thread.Sleep(2000);
            }
        }

        // ---------------------------------------------

        public static IDictionary<string,PoloniexAPI.WalletTools.IBalance> RefreshWallet () {
            try {
                IDictionary<string, PoloniexAPI.WalletTools.IBalance> wallet = client.Wallet.GetBalancesAsync().Result;
                GUI.GUIManager.UpdateWallet(wallet.ToArray());
                return wallet;
            }
            catch (Exception e) {
                ErrorLog.ReportError("Error refreshing wallet", e);
            }
            return null;
        }

        public static IList<PoloniexAPI.MarketTools.IMarketChartData> RefreshChart (CurrencyPair pair, PoloniexAPI.MarketTools.MarketPeriod period) {
            try {
                DateTime startTime = DateTime.Now.Subtract(new TimeSpan(6, 0, 0));
                DateTime endTime = DateTime.Now;
                return client.Markets.GetChartDataAsync(pair, period, startTime, endTime).Result;
            }
            catch (Exception e) {
                ErrorLog.ReportError("Error refreshing chart data", e);
            }
            return null;
        }

        public static void RefreshMarketData () {
            try {
                Task<IDictionary<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> marketDataTask = client.Markets.GetSummaryAsync();

                Dictionary<CurrencyPair, PoloniexAPI.MarketTools.IMarketData> finalData = new Dictionary<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>();
                KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>[] data = marketDataTask.Result.ToArray();
                for (int i = 0; i < data.Length; i++) {
                    finalData.Add(data[i].Key, data[i].Value);
                }
                Data.Store.MarketData = finalData;
            }
            catch (Exception e) {
                ErrorLog.ReportError("Error refreshing market data", e);
            }
        }
    }
}

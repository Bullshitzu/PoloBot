using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PoloniexAPI;
using PoloniexAPI.WalletTools;
using PoloniexBot.Data.Predictors;

namespace PoloniexBot.Trading {
    static class Manager {

        public static void Start () {
            thread = Utility.ThreadManager.Register(Run, "Trading Manager", true);
        }
        public static void Stop () {
            Utility.ThreadManager.Kill(thread);
            ClearAllPairs();
        }

        static Thread thread;

        const int SleepPeriod = 250;
        const int APICallPeriod = 500;

        static PoloniexAPI.IWallet wallet;
        static IDictionary<string, IBalance> walletState;
        public static bool RefreshRequested = false;
        public static bool RefreshTickersManually = true;

        static Utility.TSList<TPManager> tradePairs;
        static Utility.TSList<CurrencyPair> updatedPairs;

        // --------------------------------

        static void Run () {

            UpdateWallet();
            int walletUpdateCounter = 300;
            int updateMode = 0;

            while (true) {

                try {

                    if (updatedPairs != null) {

                        int updatedCount = 0;
                        while (updatedPairs.Count > 0 && updatedCount < 30) {
                            updatedCount++;

                            CurrencyPair currPair = updatedPairs[0];
                            updatedPairs.RemoveAll(item => item == currPair);

                            if (tradePairs != null && Data.Store.AllowTickerUpdate) {

                                for (int i = 0; i < tradePairs.Count; i++) {
                                    if (tradePairs[i].GetPair() == currPair) {

                                        tradePairs[i].InvokeThread(tradePairs[i].UpdatePredictors, null);
                                        tradePairs[i].InvokeThread(tradePairs[i].EvaluateAndTrade, null);
                                        UpdateWalletValue(currPair.QuoteCurrency);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    walletUpdateCounter--;

                    if (walletUpdateCounter <= 0) {
                        switch (updateMode) {
                            case 0:
                                UpdateWallet();
                                break;
                            case 1:
                                UpdateTickers();
                                break;
                        }

                        updateMode++;
                        if (updateMode >= 2) updateMode = 0;

                        walletUpdateCounter = 300;
                    }

                    if (tradePairs != null) Rules.RuleGlobalDrop.ClearUnusedPairs(tradePairs.ToArray());

                    Utility.ModuleMonitor.CheckModules();

                }
                catch (Exception e) {
                    Console.WriteLine("Error in main loop: " + e.Message + "\n" + e.StackTrace);
                }

                Utility.ThreadManager.ReportAlive("Trading.Manager");

                Thread.Sleep(10);
            }
        }

        public static TPManager GetTPManager (CurrencyPair pair) {
            TPManager[] managers = tradePairs.ToArray(); // to avoid thread-safe fuckery
            for (int i = 0; i < managers.Length; i++) {
                if (managers[i].GetPair() == pair) return managers[i];
            }
            return null;
        }
        public static TPManager[] GetAllTPManagers () {
            TPManager[] managers = tradePairs.ToArray(); // to avoid thread-safe fuckery
            return managers;
        }

        public static void UpdateTickers () {
            try {
                KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>[] data = 
                    ClientManager.client.Markets.GetSummaryAsync().Result.ToArray();

                if (data == null) return;

                for (int i = 0; i < data.Length; i++) {
                    TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(data[i].Key);
                    if (lastTicker == null) continue;

                    if (!PoloniexAPI.MarketTools.MarketData.Equal(lastTicker.MarketData, data[i].Value)) {
                        TickerChangedEventArgs tempTicker = new TickerChangedEventArgs(data[i].Key, (PoloniexAPI.MarketTools.MarketData)data[i].Value);
                        tempTicker.Timestamp = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
                        
                        Data.Store.AddTickerData(tempTicker);
                    }
                }

            }
            catch (Exception e) {
                Utility.ErrorLog.ReportError("Error refreshing market data", e);
            }
        }

        public static void NotifyTickerUpdate (CurrencyPair pair) {

            if (updatedPairs == null) updatedPairs = new Utility.TSList<CurrencyPair>();
            updatedPairs.Add(pair);
        }

        public static void RefreshTradePairs () {

            ClearAllPairs();
            
            GUI.GUIManager.ClearStrategyScreen();

            // -------------
            // Refresh market data
            // -------------

            List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>> allPairs = 
                new List<KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IMarketData>>(Data.Store.MarketData.ToArray());


            Thread.Sleep(1000);

            if (tradePairs == null) tradePairs = new Utility.TSList<TPManager>();
            tradePairs.Clear();

            // -------------
            // Add USDT/BTC
            // -------------

            AddPair(new CurrencyPair("USDT", "BTC"));

            // -------------
            // Add pairs with open positions
            // -------------

            for (int i = 0; i < allPairs.Count; i++) {
                if (Utility.TradeTracker.GetOpenPosition(allPairs[i].Key) > 0) {
                    AddPair(allPairs[i].Key);
                    allPairs.RemoveAt(i);
                    i--;
                }
            }

            // -------------
            // Filter BTC base and low price
            // -------------

            for (int i = 0; i < allPairs.Count; i++) {
                if (allPairs[i].Key.BaseCurrency != "BTC" || allPairs[i].Value.PriceLast < 0.000005) {
                    allPairs.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            // -------------
            // Sort by volume
            // -------------

            allPairs.Sort(new Utility.MarketDataComparerVolume());
            allPairs.Reverse();

            // -------------
            // Add N pairs
            // -------------

            for (int i = 0; i < allPairs.Count && tradePairs.Count < 22; i++) {
                while(true) {

                    Utility.ThreadManager.ReportAlive("Trading.Manager");

                    try {
                        Console.WriteLine("Adding " + allPairs[i] + " to traded pairs");
                        AddPair(allPairs[i].Key);
                        break;
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message + "\n" + e.StackTrace);
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        public static void RefreshTradePairsLocal () {

            ClearAllPairs();

            CurrencyPair[] newPairs = Data.Store.GetAvailableTickerPairs();

            if (tradePairs == null) tradePairs = new Utility.TSList<TPManager>();
            tradePairs.Clear();

            Utility.ThreadManager.ReportAlive("Trading.Manager");

            Console.WriteLine("Refreshing locally");
            Console.WriteLine(newPairs.Length);

            if (newPairs != null) {
                for (int i = 0; i < newPairs.Length; i++) {
                    AddPairLocal(newPairs[i]);
                }
            }
        }

        private static double CalculateVolatility (CurrencyPair pair) {

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't recalculate predictor volatility for " + pair + " - no tickers available");

            long startTime = tickers.Last().Timestamp - 10800;

            // Calculate the average price

            double sum = 0;
            int cnt = 0;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                sum += tickers[i].MarketData.PriceLast;
                cnt++;
            }

            double avgPrice = sum / cnt;

            // Calculate the stDev

            sum = 0;
            cnt = 0;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                sum += Math.Abs(avgPrice - tickers[i].MarketData.PriceLast);
                cnt++;
            }

            double stDev = sum / cnt;

            // Normalize stDev

            stDev = ((stDev - avgPrice) / avgPrice) * 100;

            return stDev;
        }

        static void CancelAllOrders (CurrencyPair pair) {
            IList<PoloniexAPI.TradingTools.IOrder> orders = PoloniexBot.ClientManager.client.Trading.GetOpenOrdersAsync(pair).Result;
            if (orders != null) {
                for (int i = 0; i < orders.Count; i++) {
                    PoloniexBot.ClientManager.client.Trading.DeleteOrderAsync(pair, orders[i].IdOrder);
                }
            }
        }
        
        public static void BlockPair (CurrencyPair pair) {
            if (SetBlockedState(pair, true)) CLI.Manager.PrintNote("Blocking trade of " + pair.QuoteCurrency + ".");
            else CLI.Manager.PrintError("Currency " + pair.QuoteCurrency + " not found among actively traded pairs!");
        }
        public static void BlockAll () {
            if (SetBlockedStateAll(true)) CLI.Manager.PrintNote("Blocking trade of all active trade pairs.");
            else CLI.Manager.PrintError("No active trade pairs!");
        }
        
        public static void ResumePair (CurrencyPair pair) {
            if (SetBlockedState(pair, false)) CLI.Manager.PrintNote("Resuming trade of " + pair.QuoteCurrency + ".");
            else CLI.Manager.PrintError("Currency " + pair.QuoteCurrency + " not found among actively traded pairs!");
        }
        public static void ResumeAll () {
            if (SetBlockedStateAll(false)) CLI.Manager.PrintNote("Resuming trade of all active trade pairs.");
            else CLI.Manager.PrintError("No active trade pairs!");
        }

        private static bool SetBlockedState (CurrencyPair pair, bool state) {
            TPManager[] tpManagers = GetAllTPManagers();
            if (tpManagers == null) return false;

            for (int i = 0; i < tpManagers.Length; i++) {
                if (tpManagers[i].GetPair() == pair) {
                    tpManagers[i].SetBlockedTrade(state);
                    return true;
                }
            }

            return false;
        }
        private static bool SetBlockedStateAll (bool state) {
            TPManager[] tpManagers = GetAllTPManagers();
            if (tpManagers == null) return false;

            for (int i = 0; i < tpManagers.Length; i++) {
                tpManagers[i].SetBlockedTrade(state);
            }

            return true;
        }
        
        public static void UpdateWallet () {
            // makes an API call to retrieve from server
            try {
                if (wallet == null) wallet = PoloniexBot.ClientManager.client.Wallet;
                IDictionary<string, IBalance> tempWallet = wallet.GetBalancesAsync().Result;
                if (tempWallet != null) {
                    walletState = tempWallet;

                    IBalance balance;
                    if (walletState.TryGetValue("USDT", out balance)) {
                        balance.BitcoinValue = (balance.QuoteAvailable + balance.QuoteOnOrders) / Strategies.BaseTrendMonitor.LastUSDTBTCPrice;
                    }

                    GUI.GUIManager.UpdateWallet(tempWallet.ToArray());
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        public static void UpdateWalletValue (string currency) {
            // update the wallet BTC value based on most recent ticker values
            // doesn't make any API calls, just recalculates based on local data
            try {
                if (walletState == null) UpdateWallet();
                else if (currency == "BTC") return;
                else {
                    IBalance balance;
                    if (walletState.TryGetValue(currency, out balance)) {
                        CurrencyPair cp = new CurrencyPair("BTC", currency);
                        TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(cp);
                        if (lastTicker != null) {
                            double currValue = lastTicker.MarketData.PriceLast;
                            double currAmount = balance.QuoteOnOrders + balance.QuoteAvailable;
                            balance.BitcoinValue = currAmount * currValue;
                            GUI.GUIManager.UpdateWallet(walletState.ToArray());
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        public static void UpdateWalletValue (string currency, double price) {
            // update the wallet BTC value based on manual price input
            // doesn't make any API calls, just recalculates based on local data
            try {
                if (walletState == null) UpdateWallet();
                else if (currency == "USDT") {
                    IBalance balance;
                    if (walletState.TryGetValue(currency, out balance)) {
                        double currValue = price;
                        double currAmount = balance.QuoteOnOrders + balance.QuoteAvailable;
                        balance.BitcoinValue = currAmount / currValue;
                        GUI.GUIManager.UpdateWallet(walletState.ToArray());
                    }
                }
                else if (currency == "BTC") return;
                else {
                    IBalance balance;
                    if (walletState.TryGetValue(currency, out balance)) {
                        double currAmount = balance.QuoteOnOrders + balance.QuoteAvailable;
                        balance.BitcoinValue = currAmount * price;
                        GUI.GUIManager.UpdateWallet(walletState.ToArray());
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        public static double GetWalletState (string currency) {
            if (walletState == null) UpdateWallet();

            IBalance balance;
            if (walletState.TryGetValue(currency, out balance)) 
                return balance.QuoteAvailable;

            return 0;
        }
        public static double GetWalletStateOrders (string currency) {
            if (walletState == null) UpdateWallet();

            IBalance balance;
            if (walletState.TryGetValue(currency, out balance))
                return balance.QuoteOnOrders;

            return 0;
        }
        
        public static IList <PoloniexAPI.TradingTools.IOrder> UpdateActiveOrders (CurrencyPair pair) {
            return PoloniexBot.ClientManager.client.Trading.GetOpenOrdersAsync(pair).Result;
        }

        public static TPManager AddPair (CurrencyPair pair) {
            if (tradePairs == null) tradePairs = new Utility.TSList<TPManager>();

            TPManager tpMan = new TPManager(pair);
            tpMan.Setup();
            tpMan.RecalculateVolatility();
            tpMan.Start();
            tradePairs.Add(tpMan);

            return tpMan;
        }
        public static void AddPairLocal (CurrencyPair pair) {
            // adds the pair without pulling any data, using only what's available from file

            if (tradePairs == null) tradePairs = new Utility.TSList<TPManager>();

            TPManager tpMan = new TPManager(pair);

            tpMan.Setup(false);
            tpMan.RecalculateVolatility();

            Console.WriteLine("Added Pair "+pair+" Locally.");

            tpMan.Start();
            tradePairs.Add(tpMan);
        }
        public static void RemovePair (CurrencyPair pair) {
            if (tradePairs == null) return;
            for (int i = 0; i < tradePairs.Count; i++) {
                if (tradePairs[i].GetPair() == pair) {
                    tradePairs[i].Stop();
                    tradePairs[i].Dispose();
                    tradePairs.RemoveAt(i);
                    break;
                }
            }
            GUI.GUIManager.RemovePairSummary(pair);
        }
        public static void ClearAllPairs () {
            if (tradePairs == null) return;

            GUI.GUIManager.ClearPairSummaries();

            for (int i = 0; i < tradePairs.Count; i++) {
                tradePairs[i].Stop();
                tradePairs[i].Dispose();
                tradePairs.RemoveAt(i);
            }
        }

        public static bool ForceBuy (CurrencyPair pair) {
            if (tradePairs == null) return false;

            for (int i = 0; i < tradePairs.Count; i++) {
                if (tradePairs[i].GetPair() == pair) {
                    tradePairs[i].ForceBuy();
                    return true;
                }
            }

            AddPair(pair).ForceBuy();

            return false;
        }
        public static bool ForceSell (CurrencyPair pair) {
            if (tradePairs == null) return false;

            for (int i = 0; i < tradePairs.Count; i++) {
                if (tradePairs[i].GetPair() == pair) {
                    tradePairs[i].ForceSell();
                    return true;
                }
            }

            return false;
        }
    }
}

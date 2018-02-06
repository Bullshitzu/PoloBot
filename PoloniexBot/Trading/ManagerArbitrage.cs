using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexAPI.WalletTools;

namespace PoloniexBot.Trading {
    static class ManagerArbitrage {

        private const string base1 = "BTC";
        private static string[] base2 = { "ETH", "XMR" };

        // --------------------------------------------------------------

        public static void Start () {
            thread = Utility.ThreadManager.Register(Run, "Trading Manager", true);
        }
        public static void Stop () {
            Utility.ThreadManager.Kill(thread);
            ClearAllPairs();
        }

        static Thread thread;

        // --------------------------------------------------------------

        const int SleepPeriod = 250;
        public const int APICallPeriod = 500;

        const int MaxAltCount = 20;

        public const double MinProfitMargin = 0;

        static PoloniexAPI.IWallet wallet;
        static IDictionary<string, IBalance> walletState;

        static List<TPManagerArbitrage> pairMonitors;

        public static bool ShouldUpdateWallet = true;

        // --------------------------------------------------------------

        private static void Run () {
            while (true) {
                try {
                    switch (ShouldUpdateWallet) {
                        case true:
                            UpdateWallet();
                            ShouldUpdateWallet = false;
                            break;
                        case false:
                            UpdateAllOrderBooks();
                            UpdateAllPairMonitors();
                            break;
                    }

                    Utility.ModuleMonitor.CheckModules();

                }
                catch (Exception e) {
                    Console.WriteLine("Error in main loop: " + e.Message + "\n" + e.StackTrace);
                }

                Utility.ThreadManager.ReportAlive("Trading.Manager");

                Thread.Sleep(APICallPeriod);
            }
        }

        // --------------------------------------------------------------

        public static void RefreshTradePairs () {

            ClearAllPairs();

            GUI.GUIManager.ClearStrategyScreen();

            // ---------------------------------

            if (pairMonitors == null) pairMonitors = new List<TPManagerArbitrage>();
            pairMonitors.Clear();

            CurrencyPair[] triageAlts = GetTradePairs();

            for (int i = 0; i < triageAlts.Length && i < MaxAltCount; i++) {
                AddPairMonitor(triageAlts[i]);
            }
        }
        public static CurrencyPair[] GetTradePairs () {

            CurrencyPair[] allPairs = Data.Store.MarketData.Keys.ToArray();

            List<CurrencyPair> pairsBase1 = new List<CurrencyPair>();
            List<CurrencyPair> pairsBase2 = new List<CurrencyPair>();

            for (int i = 0; i < allPairs.Length; i++) {
                if (allPairs[i].BaseCurrency == base1) pairsBase1.Add(allPairs[i]);
                for (int j = 0; j < base2.Length; j++) {
                    if (allPairs[i].BaseCurrency == base2[j]) pairsBase2.Add(allPairs[i]);
                }
            }

            List<CurrencyPair> commonPairs = new List<CurrencyPair>();
            for (int i = 0; i < pairsBase1.Count; i++) {
                for (int j = 0; j < pairsBase2.Count; j++) {
                    if (pairsBase1[i].QuoteCurrency == pairsBase2[j].QuoteCurrency) {
                        commonPairs.Add(pairsBase2[j]);
                        break;
                    }
                }
            }

            return commonPairs.ToArray();
        }

        public static void ClearAllPairs () {
            if (pairMonitors == null) return;

            GUI.GUIManager.ClearPairSummaries();
            for (int i = 0; i < pairMonitors.Count; i++) {
                pairMonitors[i].Dispose();
            }
            pairMonitors.Clear();
        }

        public static void AddPairMonitor (CurrencyPair quoteCurrency) {
            if (pairMonitors == null) pairMonitors = new List<TPManagerArbitrage>();

            TPManagerArbitrage pairMonitorTemp = new TPManagerArbitrage(quoteCurrency.QuoteCurrency, base1, quoteCurrency.BaseCurrency);
            pairMonitorTemp.Setup();

            pairMonitors.Add(pairMonitorTemp);
        }

        public static TPManagerArbitrage GetPairMonitor (CurrencyPair pair) {
            if (pairMonitors == null) return null;

            TPManagerArbitrage[] monitors = pairMonitors.ToArray(); // to avoid thread-safe fuckery
            for (int i = 0; i < monitors.Length; i++) {
                if (monitors[i].pair1.QuoteCurrency == pair.QuoteCurrency) return monitors[i];
            }
            return null;
        }
        public static TPManagerArbitrage[] GetPairMonitors () {
            return pairMonitors.ToArray();
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

        // --------------------------------------------------------------

        private static void UpdateWallet () {
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
        private static void UpdateWalletValue (string currency, double price) {
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

        private static void UpdateAllOrderBooks () {
            // makes an API call to retrieve from server
            try {
                IDictionary<CurrencyPair, PoloniexAPI.MarketTools.IOrderBook> tempOrderBooks = ClientManager.client.Markets.GetOpenOrdersAsync(5).Result;
                Data.Store.UpdateOrderBooks(tempOrderBooks);

                // update wallet values based on orderBooks
                KeyValuePair<CurrencyPair, PoloniexAPI.MarketTools.IOrderBook>[] orderBooksArray = tempOrderBooks.ToArray();
                for (int i = 0; i < orderBooksArray.Length; i++) {
                    if (orderBooksArray[i].Key.BaseCurrency == base1) {
                        UpdateWalletValue(orderBooksArray[i].Key.QuoteCurrency, orderBooksArray[i].Value.BuyOrders.First().PricePerCoin);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }

        private static void UpdateAllPairMonitors () {
            if (pairMonitors == null) return;

            for (int i = 0; i < pairMonitors.Count; i++) {
                pairMonitors[i].Update();
            }
        }

    }
}

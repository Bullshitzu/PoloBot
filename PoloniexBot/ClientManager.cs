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

            CLI.Manager.PrintNote("Initializing Client");
            client = new PoloniexClient(apiKey[0], apiKey[1]);

            // Clock
            CLI.Manager.PrintNote("Calculating Clock Offset");
            Utility.DateTimeHelper.RecalculateClockOffset();

            client.Live.Start();

            // Trollbox
            // CLI.Manager.PrintNote("Subscribing To Trollbox");
            // client.Live.SubscribeToTrollboxAsync();
            // client.Live.OnTrollboxMessage += new EventHandler<TrollboxMessageEventArgs>(Windows.GUIManager.trollboxWindow.RecieveMessage);

            CLI.Manager.PrintNote("Pulling Market Summary");
            RefreshMarketData();

            // Ticker Feed
            CLI.Manager.PrintNote("Subscribing To Ticker Feed");
            client.Live.SubscribeToTickerAsync();
            client.Live.OnTickerChanged += new EventHandler<TickerChangedEventArgs>(Windows.GUIManager.tickerFeedWindow.RecieveMessage);

            Trading.Manager.Start();
            ThreadManager.Register(Trading.Manager.RefreshTradePairs, "TP Refresh", true);
        }

        // ---------------------------------------------

        public static IDictionary<string,PoloniexAPI.WalletTools.IBalance> RefreshWallet () {
            try {
                IDictionary<string, PoloniexAPI.WalletTools.IBalance> wallet = client.Wallet.GetBalancesAsync().Result;
                Windows.GUIManager.accountStatusWindow.UpdateBalance(wallet);
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
                    if (data[i].Key.BaseCurrency == "BTC") {
                        finalData.Add(data[i].Key, data[i].Value);
                    }
                }
                Data.Store.MarketData = finalData;
                Windows.GUIManager.tickerFeedWindow.UpdateMarketData();
            }
            catch (Exception e) {
                ErrorLog.ReportError("Error refreshing market data", e);
            }
        }
    }
}

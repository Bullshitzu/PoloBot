using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot {
    static class Simulation {

        static Simulation () {
            wallet = (PoloniexAPI.WalletTools.WalletSimulated)PoloniexBot.ClientManager.client.Wallet;
        }

        static PoloniexAPI.WalletTools.WalletSimulated wallet;
        public static void ResetWallet () {
            wallet.Reset();
        }

        public static void PostOrder (CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote) {
            wallet.DoTransaction(currencyPair, type, pricePerCoin, amountQuote);
        }

        // ----------------------

        public static void SimulateAll () {

            // -----------------------------
            // load trade data
            // -----------------------------

            Utility.TSList<Utility.TSList<TickerChangedEventArgs>> fullTickerList = Data.Store.LoadTradeData(false);
            if (fullTickerList == null) throw new Exception("Data store loaded NULL tickers");

            List<TickerChangedEventArgs> allTickers = new List<TickerChangedEventArgs>();
            for (int i = 0; i < fullTickerList.Count; i++) {
                for (int j = 0; j < fullTickerList[i].Count; j++) {
                    allTickers.Add(fullTickerList[i][j]);
                }
            }

            allTickers.Sort();

            // -----------------------------
            // setup optimizers
            // -----------------------------

            // todo: ...

            // -----------------------------
            // setup TPManagers
            // -----------------------------

            List<Trading.TPManager> tpManagers = new List<Trading.TPManager>();

            for (int i = 0; i < fullTickerList.Count; i++) {
                CurrencyPair pair = fullTickerList[i].First().CurrencyPair;

                Data.Store.allowUpdatePairs.Add(pair);
                Trading.TPManager currTPMan = new Trading.TPManager(pair);

                tpManagers.Add(currTPMan);
            }

            // -----------------------------
            // genetic loop
            // -----------------------------

            int iterations = 0;
            double bestScore = 0;

            bool lastImproved = false;

            while (true) {

                CLI.Manager.PrintNote("Starting Training Session (" + bestScore.ToString("F8") + ")");

                // reset the system

                wallet.Reset();
                Trading.Manager.UpdateWallet();
                
                Data.Store.ClearTickerData();
                Utility.TradeTracker.ClearAll();

                // add first 10000

                int startIndex = 0;
                long endTime = allTickers.First().Timestamp + 25200; // 7 hours

                for (int i = 0; i < allTickers.Count; i++) {
                    if (allTickers[i].Timestamp > endTime) break;
                    AddTicker(allTickers[i], null, false);
                    startIndex = i;
                }

                // rebuild TPManagers

                for (int i = 0; i < tpManagers.Count; i++) {
                    tpManagers[i].Reset();
                }

                // add all tickers

                for (int i = startIndex; i < allTickers.Count; i++) {
                    AddTicker(allTickers[i], tpManagers, true);



                    if (i % 100 == 0) {
                        float percent = (((float)i) / allTickers.Count) * 100;
                        Console.WriteLine("Progress: " + percent.ToString("F2") + "%");
                    }
                }

                // score performance, revert or save

                IDictionary<string, PoloniexAPI.WalletTools.IBalance> balances = wallet.GetBalancesAsync().Result;
                KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] balancesArray = balances.ToArray();

                double btcValue = 0;
                for (int z = 0; z < balancesArray.Length; z++) {
                    btcValue += balancesArray[z].Value.BitcoinValue;
                }

                if (btcValue > bestScore) {
                    // save result (with optimizer values)

                    // todo: 
                    // SaveResultsToFile(btcValue, optimizers);

                    bestScore = btcValue;

                    lastImproved = true;

                    CLI.Manager.PrintNote("New Best: " + bestScore.ToString("F8") + " BTC");
                }
                else {
                    // todo: revert optimizers
                    /*
                    for (int i = 0; i < optimizers.Length; i++) {
                        optimizers[i].Revert();
                    }
                    */
                    lastImproved = false;
                }

                iterations++;

                return;

            }
        }
        private static void AddTicker (TickerChangedEventArgs ticker, List<Trading.TPManager> tpManagers, bool evaluate) {
            
            Data.Store.AddTickerData(ticker);

            if (evaluate) {
                for (int i = 0; i < tpManagers.Count; i++) {
                    if (ticker.CurrencyPair == tpManagers[i].GetPair()) {

                        tpManagers[i].UpdatePredictors();
                        tpManagers[i].EvaluateAndTrade();

                        Trading.Manager.UpdateWalletValue(ticker.CurrencyPair.QuoteCurrency);

                        return;
                    }
                }
            }
        }

        public static void ReportTrade (bool profit, double[] variables) {

            // meanRev,
            // meanRevGlobal,
            // macd
            // price delta 1
            // price delta 2

            string line = profit + ":";
            for (int i = 0; i < variables.Length; i++) {
                line += variables[i].ToString("F8", System.Globalization.CultureInfo.InvariantCulture);
                if (i + 1 < variables.Length) line += ":";
            }

            if (debugLines == null) debugLines = new List<string>();
            debugLines.Add(line);


            Utility.FileManager.SaveFile("Logs/trades.data", debugLines.ToArray());

        }

        private static List<string> debugLines;

        private static double RunSimulationInstance (CurrencyPair pair, Trading.TPManager tempTPManager, TickerChangedEventArgs[] tickers) {

            wallet.Reset();
            Trading.Manager.UpdateWallet();

            Data.Store.ClearTickerData();

            for (int z = 0; z < 100; z++) {
                Data.Store.AddTickerData(tickers[z]);
            }

            Utility.TradeTracker.ClearAll();
            tempTPManager.Reset();

            for (int z = 100; z < tickers.Length; z++) {

                Data.Store.AddTickerData(tickers[z]);

                tempTPManager.UpdatePredictors();
                tempTPManager.EvaluateAndTrade();
                Trading.Manager.UpdateWalletValue(pair.QuoteCurrency);

                if (z % 5000 == 0) Console.WriteLine("Progress: " + z + " / " + tickers.Length);
            }

            IDictionary<string, PoloniexAPI.WalletTools.IBalance> balances = wallet.GetBalancesAsync().Result;
            KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] balancesArray = balances.ToArray();

            double btcValue = 0;
            for (int z = 0; z < balancesArray.Length; z++) {
                btcValue += balancesArray[z].Value.BitcoinValue;
            }

            return btcValue;

        }

        // ----------------------

    }
}

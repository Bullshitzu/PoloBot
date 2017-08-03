using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Data.GeneticOptimizer;

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

        #region Test Variables

        static double[] varsMeanRevTrigger = { 2, 2.5, 3 };
        static double[] varsMACDTrigger = { 1.5 };

        static double[] varsMACDTrendTrigger = { 1, 0.75, 0.5, 0.25, 0 };
        static double[] varsPriceDeltaTrigger = { 0, 0.25, 0.5, 0.75, 1, 1.25 };

        static double[] varsSellBandFactor = { 8, 9, 10 };
        static double[] varsSellBandSize = { 1.5, 2, 2.5 };
        static double[] varsSellBandOffset = { 0.25, 0.5, 0.75 };

        static double[] varsMeanRevTimespan = { 12600 };
        static double[] varsPriceDeltaTimespan = { 3600, 5400, 7200 };

        static double[] varsMACDEMA = { 1500 };
        static double[] varsMACDSMA = { 2700 };

        static double[] varsSellBandPriceTriggerMult = { 0, 0.2, 0.4, 0.6, 0.8, 1 };

        static double[] varsPatternMatchBuyTrigger = { 0.5, 0.6, 0.7, 0.8 };

        #endregion

        // ----------------------

        private struct ResultSetSimple : IComparable<ResultSetSimple> {

            public ResultSetSimple (string[] lines, double result) {
                this.lines = lines;
                this.result = result;
            }

            private string[] lines;
            private double result;

            public int CompareTo (ResultSetSimple other) {
                return result.CompareTo(other.result);
            }

            public string[] GetLines () {
                return lines;
            }
        }

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
            // setup optimizers
            // -----------------------------

            Optimizer[] optimizers = new Optimizer[] {
                new OptimizerMACD(-0.08489389, 0.05),
                new OptimizerMACDSMA(691.74825200, 50),
                new OptimizerMACDEMA(382.83748495, 50),
                // new OptimizerADX(52.3, 5),
                new OptimizerMeanRev(1.70222881, 0.15),
                new OptimizerSellBandPriceTriggerMult(0.95938815, 0.1),
                new OptimizerSellBandFactor(2.67627255, 0.15),
                new OptimizerSellBandSize(2.86135387, 0.25),
                new OptimizerSellBandOffset(-0.56772173, 0.15)
            };

            // -----------------------------
            // genetic loop
            // -----------------------------

            int iterations = 0;
            double bestScore = 0;

            while (true) {

                CLI.Manager.PrintNote("Starting Training Session (" + bestScore.ToString("F8") + ")");

                // reset the system

                wallet.Reset();
                Trading.Manager.UpdateWallet();
                
                Data.Store.ClearTickerData();
                Utility.TradeTracker.ClearAll();

                // add first 1000

                for (int i = 0; i < 1000; i++) {
                    AddTicker(allTickers[i], null, false);
                }

                // rebuild TPManagers

                for (int i = 0; i < tpManagers.Count; i++) {
                    tpManagers[i].Reset();
                }

                // initialize or mutate optimizers

                for (int i = 0; i < optimizers.Length; i++) {
                    if (iterations == 0) optimizers[i].InitializeValue();
                    else optimizers[i].Mutate();
                }

                // add all tickers

                for (int i = 0; i < allTickers.Count; i++) {
                    AddTicker(allTickers[i], tpManagers, true);

                    if (i % 10000 == 0) {
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
                    SaveResultsToFile(btcValue, optimizers);
                    bestScore = btcValue;

                    CLI.Manager.PrintNote("New Best: " + bestScore.ToString("F8") + " BTC");
                }
                else {
                    // revert optimizers
                    for (int i = 0; i < optimizers.Length; i++) {
                        optimizers[i].Revert();
                    }
                }

                iterations++;

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

        public static void SimulatePair (CurrencyPair pair) {

            Utility.TSList<Utility.TSList<TickerChangedEventArgs>> allTickers = Data.Store.LoadTradeData(false);
            if (allTickers == null) throw new Exception("Data store loaded NULL tickers");

            // -------------------------------
            // Prepare training data
            // -------------------------------

            TickerChangedEventArgs[] tickers = null;
            for (int i = 0; i < allTickers.Count; i++) {
                if (allTickers[i] == null || allTickers[i].Count == 0) continue;
                if (allTickers[i][0].CurrencyPair == pair) {
                    tickers = allTickers[i].ToArray();
                    break;
                }
            }

            if (tickers == null) throw new Exception("Tickers for pair " + pair + " not found!");

            Data.Store.allowUpdatePairs.Add(pair);

            Trading.TPManager tempTPManager = new Trading.TPManager(pair);

            // -------------------------------
            // Prepare optimizers
            // -------------------------------

            OptimizerMeanRev optMeanRev = new OptimizerMeanRev(varsMeanRevTrigger);
            OptimizerMACD optMACD = new OptimizerMACD(varsMACDTrigger);

            OptimizerMACDTrend optMACDTrend = new OptimizerMACDTrend(varsMACDTrendTrigger);

            OptimizerPriceDelta optPriceDelta1 = new OptimizerPriceDelta(varsPriceDeltaTrigger, 1);
            OptimizerPriceDelta optPriceDelta2 = new OptimizerPriceDelta(varsPriceDeltaTrigger, 2);
            OptimizerPriceDelta optPriceDelta3 = new OptimizerPriceDelta(varsPriceDeltaTrigger, 3);

            OptimizerSellBandFactor optSellBandFactor = new OptimizerSellBandFactor(varsSellBandFactor);
            OptimizerSellBandSize optSellBandSize = new OptimizerSellBandSize(varsSellBandSize);
            OptimizerSellBandOffset optSellBandOffset = new OptimizerSellBandOffset(varsSellBandOffset);

            OptimizerMeanRevTimespan optMeanRevTimespan = new OptimizerMeanRevTimespan(varsMeanRevTimespan);
            OptimizerPriceDeltaTimespan optPriceDeltaTimespan = new OptimizerPriceDeltaTimespan(varsPriceDeltaTimespan);

            OptimizerMACDSMA optMacdSMA = new OptimizerMACDSMA(varsMACDSMA);
            OptimizerMACDEMA optMacdEMA = new OptimizerMACDEMA(varsMACDEMA);

            OptimizerSellBandPriceTriggerMult optSellBandPriceTriggerMult = new OptimizerSellBandPriceTriggerMult(varsSellBandPriceTriggerMult);

            OptimizerPatternMatchBuyTrigger optPatternMatchBuyTrigger = new OptimizerPatternMatchBuyTrigger(varsPatternMatchBuyTrigger);

            // -------------------------------

            Optimizer[] optimizers = new Optimizer[] { optMACD, optMacdSMA, optMacdEMA };

            // -------------------------------
            // Run training iterations
            // -------------------------------

            List<ResultSetSimple> results = new List<ResultSetSimple>();

            bool breakMain = false;
            while(!breakMain) {

                System.Threading.Thread.Sleep(200);

                // set variables
                string tempString = "";
                for (int i = 0; i < optimizers.Length; i++) {
                    optimizers[i].SetValue();
                    tempString += optimizers[i].GetIndex() + " ";
                }

                // CLI notification (start)
                CLI.Manager.PrintLog("Starting Training Set: - " + tempString);

                // Call RunSimulationInstance, get result
                double result = RunSimulationInstance(pair, tempTPManager, tickers);

                // CLI notification (end)
                CLI.Manager.PrintLog("Training Set Complete: " + result.ToString("F8") + " BTC");

                // Add the data to list
                List<string> lines = new List<string>();
                
                lines.Add("Pair: " + pair);
                lines.Add("Tickers: " + tickers.Length);
                lines.Add("Result: " + result.ToString("F8") + " BTC");

                // add variable setup to string list
                for (int i = 0; i < optimizers.Length; i++) {
                    lines.Add(optimizers[i].ToString());
                }

                // save the result to the list
                results.Add(new ResultSetSimple(lines.ToArray(), result));

                // save the data
                SaveResultsToFile(results.ToArray());

                // increment values and recognize break
                int incrementIndex = 0;
                while (true) {

                    if (optimizers[incrementIndex].IterateValue()) {
                        incrementIndex++;

                        if (incrementIndex >= optimizers.Length) {
                            breakMain = true;
                            break;
                        }
                    }
                    else break;
                }
            }

            // save the data
            SaveResultsToFile(results.ToArray());

            CLI.Manager.PrintLog("Training completed!");
        }

        public static void SimulateIdeal (CurrencyPair pair) {

            Utility.TSList<Utility.TSList<TickerChangedEventArgs>> allTickers = Data.Store.LoadTradeData(false);
            if (allTickers == null) throw new Exception("Data store loaded NULL tickers");

            // -------------------------------
            // Prepare training data
            // -------------------------------

            TickerChangedEventArgs[] tickers = null;
            for (int i = 0; i < allTickers.Count; i++) {
                if (allTickers[i] == null || allTickers[i].Count == 0) continue;
                if (allTickers[i][0].CurrencyPair == pair) {
                    tickers = allTickers[i].ToArray();
                    break;
                }
            }

            if (tickers == null) throw new Exception("Tickers for pair " + pair + " not found!");

            // -------------------------------

            bool searchingBuy = true;
            bool searchingSell = false;

            double buyPrice = double.MaxValue;
            double sellPrice = double.MinValue;

            int buyIndex = 0;
            int sellIndex = 0;

            List<int> buyIndexes = new List<int>();
            List<int> sellIndexes = new List<int>();

            double minGap = Trading.Rules.RuleMinimumSellPrice.ProfitFactor;

            for (int i = 0; i < tickers.Length; i++) {
                double currPrice = tickers[i].MarketData.PriceLast;

                if (currPrice < buyPrice) {
                    buyPrice = currPrice;
                    buyIndex = i;
                }
                if (currPrice > sellPrice) {
                    sellPrice = currPrice;
                    sellIndex = i;
                }

                if (searchingBuy) {
                    if (sellPrice / buyPrice > minGap && sellIndex > buyIndex) {
                        buyIndexes.Add(buyIndex);

                        buyIndex = i;
                        buyPrice = currPrice;

                        searchingBuy = false;
                        searchingSell = true;
                    }
                }
                else if (searchingSell) {
                    if (sellPrice / currPrice > minGap) {
                        sellIndexes.Add(sellIndex);

                        sellIndex = i;
                        sellPrice = currPrice;

                        buyIndex = i;
                        buyPrice = currPrice;

                        searchingBuy = true;
                        searchingSell = false;
                    }
                }
            }

            // -------------------------------

            RunSimulationIdeal(pair, tickers, buyIndexes, sellIndexes);

        }
        private static void RunSimulationIdeal (CurrencyPair pair, TickerChangedEventArgs[] tickers, List<int> buyIndexes, List<int> sellIndexes) {

            if (tickers == null) throw new Exception("Tickers for pair " + pair + " not found!");

            Data.Store.allowUpdatePairs.Add(pair);

            Trading.TPManager tempTPManager = new Trading.TPManager(pair);

            // ----------------------------------

            List<ResultSetSimple> results = new List<ResultSetSimple>();

            bool breakMain = false;
            while (!breakMain) {

                System.Threading.Thread.Sleep(200);

                // CLI notification (start)
                CLI.Manager.PrintLog("Starting Training Set");

                // Call RunIdealSimulationInstance, get result
                // double result = RunIdealSimulationInstance(pair, tempTPManager, tickers, buyIndexes, sellIndexes);
                double result = RunIdealMapPatterns(pair, tempTPManager, tickers, buyIndexes, sellIndexes);

                // CLI notification (end)
                CLI.Manager.PrintLog("Training Set Complete: " + result.ToString("F8") + " BTC");

                // Add the data to list
                List<string> lines = new List<string>();

                lines.Add("Pair: " + pair);
                lines.Add("Tickers: " + tickers.Length);
                lines.Add("Result: " + result.ToString("F8") + " BTC");

                // save the result to the list
                results.Add(new ResultSetSimple(lines.ToArray(), result));

                // save the data
                SaveResultsToFile(results.ToArray());

                break;
            }

            CLI.Manager.PrintLog("Training completed!");


        }
        private static double RunIdealSimulationInstance (CurrencyPair pair, Trading.TPManager tempTPManager, TickerChangedEventArgs[] tickers, List<int> buyIndexes, List<int> sellIndexes) {

            wallet.Reset();
            Trading.Manager.UpdateWallet();

            Data.Store.ClearTickerData();

            int buyIndexesIndex = 0;
            int sellIndexesIndex = 0;

            for (int z = 0; z < 100; z++) {
                Data.Store.AddTickerData(tickers[z]);

                if (buyIndexes[buyIndexesIndex] == z) buyIndexesIndex++;
                if (sellIndexes[sellIndexesIndex] == z) sellIndexesIndex++;
            }

            Utility.TradeTracker.ClearAll();
            tempTPManager.Reset();

            for (int z = 100; z < tickers.Length; z++) {

                Data.Store.AddTickerData(tickers[z]);

                tempTPManager.UpdatePredictors();

                for (int i = buyIndexesIndex; i < buyIndexes.Count; i++) {
                    if (buyIndexes[i] > z) break;
                    if (buyIndexes[i] == z) {
                        tempTPManager.ForceBuy();
                        buyIndexesIndex = i;
                    }
                }
                for (int i = sellIndexesIndex; i < sellIndexes.Count; i++) {
                    if (sellIndexes[i] > z) break;
                    if (sellIndexes[i] == z) {
                        tempTPManager.ForceSell();
                        sellIndexesIndex = i;
                    }
                }

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
        private static double RunIdealMapPatterns (CurrencyPair pair, Trading.TPManager tempTPManager, TickerChangedEventArgs[] tickers, List<int> buyIndexes, List<int> sellIndexes) {

            wallet.Reset();
            Trading.Manager.UpdateWallet();

            Data.Store.ClearTickerData();

            int buyIndexesIndex = 0;
            int sellIndexesIndex = 0;

            for (int z = 0; z < 100; z++) {
                Data.Store.AddTickerData(tickers[z]);

                if (buyIndexes[buyIndexesIndex] == z) buyIndexesIndex++;
                if (sellIndexes[sellIndexesIndex] == z) sellIndexesIndex++;
            }

            Utility.TradeTracker.ClearAll();
            tempTPManager.Reset();

            for (int z = 100; z < tickers.Length; z++) {

                Data.Store.AddTickerData(tickers[z]);

                tempTPManager.UpdatePredictors();

                for (int i = buyIndexesIndex; i < buyIndexes.Count; i++) {
                    if (buyIndexes[i] > z) break;
                    if (buyIndexes[i] == z) {
                        
                        // Create and map the new pattern
                        Data.PatternMatching.Pattern p = Data.PatternMatching.Manager.GeneratePattern(Data.Store.GetTickerData(pair));
                        List<double> patternData = new List<double>(p.movement);
                        
                        Trading.Strategies.TestStrategy testStrat = (Trading.Strategies.TestStrategy)tempTPManager.GetStrategy();
                        patternData.AddRange(testStrat.GetPredictorValues());

                        p.movement = patternData.ToArray();
                        Data.PatternMatching.Manager.MapPattern(p, Trading.MarketAction.Buy);

                        // execute the buy

                        tempTPManager.ForceBuy();
                        buyIndexesIndex = i;
                    }
                }
                for (int i = sellIndexesIndex; i < sellIndexes.Count; i++) {
                    if (sellIndexes[i] > z) break;
                    if (sellIndexes[i] == z) {
                        // Data.PatternMatching.Manager.MapPattern(Data.Store.GetTickerData(pair), Trading.MarketAction.Sell);
                        tempTPManager.ForceSell();
                        sellIndexesIndex = i;
                    }
                }

                Trading.Manager.UpdateWalletValue(pair.QuoteCurrency);

                if (z % 5000 == 0) Console.WriteLine("Progress: " + z + " / " + tickers.Length);
            }

            // save mapped patterns
            Data.PatternMatching.Manager.SaveToFile();

            // Update wallet balance
            IDictionary<string, PoloniexAPI.WalletTools.IBalance> balances = wallet.GetBalancesAsync().Result;
            KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] balancesArray = balances.ToArray();

            double btcValue = 0;
            for (int z = 0; z < balancesArray.Length; z++) {
                btcValue += balancesArray[z].Value.BitcoinValue;
            }

            return btcValue;

        }

        private static void SaveResultsToFile (ResultSetSimple[] results) {

            List<ResultSetSimple> tempList = new List<ResultSetSimple>(results);
            
            tempList.Sort();
            tempList.Reverse();

            List<string> tempLines = new List<string>();

            for (int i = 0; i < tempList.Count; i++) {
                tempLines.Add("");
                tempLines.AddRange(tempList[i].GetLines());
                tempLines.Add("");
            }

            Utility.FileManager.SaveFileConcat("testResults.data", tempLines.ToArray());
        }
        private static void SaveResultsToFile (double result, Optimizer[] optimizers) {
            if (optimizers == null) return;

            List<string> lines = new List<string>();

            lines.Add("");
            lines.Add("Result: " + result.ToString("F8"));
            for (int i = 0; i < optimizers.Length; i++) {
                lines.Add(optimizers[i].ToStringSimple());
            }
            lines.Add("");

            Utility.FileManager.SaveFile("testResults.data", lines.ToArray());
        }

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
    }
}

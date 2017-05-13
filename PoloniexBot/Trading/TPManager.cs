using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PoloniexBot.Data;
using PoloniexAPI;
using Utility;

namespace PoloniexBot.Trading {
    class TPManager : IDisposable {

        private const float BuyTrigger = 1.2f; // 12% below 1 hour SMA

        private const double minTradeAmount = 0.0001;

        private double volatilityScore = 0;
        private double currentScore = 0;

        private double minimumSellPrice = 0;
        private double minimumSellPriceFactor = 1.005;

        private double maximumPrice = 0;
        private double maximumPriceFactor = 0.995;

        private long lastTradeTime = 0;
        private long lastSellTime = 0;

        private CurrencyPair pair;
        private Thread thread;

        private Data.Predictors.MeanReversion meanRevPredictor;

        #region Basic Setup
        // All executed in the main thread

        public Predictor[] GetPredictors () {
            return new Predictor[] { meanRevPredictor };
        }

        public TPManager (CurrencyPair pair) {
            this.pair = pair;
            invokeQueue = new TSList<InvokePair>();

            meanRevPredictor = new Data.Predictors.MeanReversion(pair);
        }
        public void Setup (bool pullTickerHistory = true) {

            double openPos = 0;
            TradeTracker.GetOpenPosition(pair, ref openPos);
            minimumSellPrice = openPos * minimumSellPriceFactor;

            if (pullTickerHistory) {
                Console.WriteLine("Pulling ticker history");
                Data.Store.PullTickerHistory(pair);
                Console.WriteLine("done");
            }

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            // note: call setup on predictors that need it

            // now recalculate history
            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                meanRevPredictor.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) ThreadManager.ReportAlive();
            }
        }
        public void RecalculateVolatility () {
            // calculate the standard price deviation in past 6 hours (in % relative to price of last ticker)

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't recalculate predictor volatility for " + pair + " - no tickers available");

            long startTime = tickers.Last().Timestamp - 21600;

            double sum = 0;
            int sumCount = 0;

            double oldPrice = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                oldPrice = tickers[i].MarketData.PriceLast;

                sum += tickers[i].MarketData.PriceLast;
                sumCount++;
            }

            double avgPrice = sum / sumCount;

            sum = 0;
            sumCount = 0;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                sum += System.Math.Abs(avgPrice - tickers[i].MarketData.PriceLast);
                sumCount++;
            }

            double stDev = sum / sumCount;
            double tempScore = (stDev / avgPrice) * 20;

            // f(x) = (log(x^3) + 4)/5
            double currencyUse = System.Math.Pow(tempScore, 3); // (Math.Log(Math.Pow(tempScore, 3)) + 4) / 5;
            if (currencyUse < 0) currencyUse = 0;
            if (currencyUse > 0.8) currencyUse = 0.8;

            // now see currency change in past ~6 hours

            double priceDiff = ((tickers.Last().MarketData.PriceLast - oldPrice) / oldPrice) * 100;
            priceDiff += 5;

            if (priceDiff < 0) priceDiff = 0;
            else priceDiff = 1;

            volatilityScore = currencyUse * priceDiff * 10;
            if (volatilityScore < 0) volatilityScore = 0;
            if (volatilityScore > 0.3) volatilityScore = 0.3;

            // DEBUG
            // volatilityScore = 1;

        }
        public void Start () {
            thread = ThreadManager.Register(Run, "Pair " + pair.ToString(" / "), true, 200);
        }
        public void Stop () {
            ThreadManager.Kill(thread);
        }
        #endregion

        public CurrencyPair GetPair () {
            return pair;
        }

        private void Run () {
            while (true) {
                try {
                    while (true) {

                        #region Resolve Invocations
                        while (invokeQueue.Count > 0) {
                            try {
                                InvokePair invokePair = invokeQueue.First();
                                invokeQueue.Remove(invokePair);
                                invokePair.method(invokePair.parameters);
                            }
                            catch (Exception e) {
                                Console.WriteLine("Invocation Error - " + e.Message + "\n" + e.StackTrace);
                                ErrorLog.ReportError(e);
                            }
                        }
                        #endregion

                        Utility.ThreadManager.ReportAlive();
                        Thread.Sleep(250);
                    }
                }
                catch (ThreadAbortException) { // thread was aborted, this is normal
                    Dispose();
                    return;
                }
                catch (Exception e) { // this is not normal, log the error
                    ErrorLog.ReportErrorSilent(e);
                }
            }
        }

        public void Dispose () {
            // todo: dispose...
        }

        #region Invoke Functionality
        public delegate void InvokeMethod (params object[] parameters);
        private struct InvokePair {
            public InvokeMethod method;
            public object[] parameters;
            public InvokePair (InvokeMethod method, object[] parameters) {
                this.method = method;
                this.parameters = parameters;
            }
        }
        TSList<InvokePair> invokeQueue;
        public void InvokeThread (InvokeMethod method, params object[] parameters) {
            invokeQueue.Add(new InvokePair(method, parameters));
        }

        // Invoke Methods Here
        public void UpdatePredictors (params object[] parameters) {
            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            RecalculateVolatility();

            meanRevPredictor.Recalculate(tickers);
        }
        public void RecalculateScore (params object[] parameters) {

            ResultSet.Variable tempVar;

            double price = 0;
            double meanRev = 0;

            if (meanRevPredictor.GetLastResult().variables.TryGetValue("price", out tempVar)) price = tempVar.value;
            if (meanRevPredictor.GetLastResult().variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;

            double score = meanRev;
            if (double.IsNaN(score) || double.IsInfinity(score)) score = 0;

            if (price > maximumPrice) maximumPrice = price;
            TradeTracker.UpdateOpenPosition(pair, price);

            currentScore = score;

        }
        public void EvaluateAndTrade (params object[] parameters) {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;

            if (lastTicker.Timestamp - lastTradeTime < 30) return;
            // to create a minimum 30 second delay between individual trades
            // prevents multiple buy orders

            double quoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            if (quoteAmount >= minTradeAmount) {
                double adjustedPrice = lastTicker.MarketData.OrderTopBuy;
                double baseAmount = quoteAmount * adjustedPrice;

                if (adjustedPrice < minimumSellPrice) return; // note: blocks selling if not profitable
                else if (adjustedPrice < maximumPrice * maximumPriceFactor && baseAmount >= minTradeAmount) { // sell quote currency
                    // -----------------------------
                    Console.WriteLine("Attempting Sell On Pair " + pair);
                    Console.WriteLine("Price: " + adjustedPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
                    // -----------------------------
                    Task<ulong> postOrderTask = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, adjustedPrice, quoteAmount);
                    ulong id = postOrderTask.Result;

                    if (id == 0) {
                        Console.WriteLine("Error making sale");
                    }
                    else {
                        Utility.TradeTracker.ReportSell(pair, quoteAmount, adjustedPrice);

                        lastTradeTime = lastTicker.Timestamp;
                        lastSellTime = lastTicker.Timestamp;
                        minimumSellPrice = 0;
                    }
                }
            }
            else if (currentScore >= BuyTrigger && lastTicker.Timestamp - lastSellTime > 1800) {
                double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
                if (currQuoteAmount < minTradeAmount) {
                    double baseAmount = Manager.GetWalletState(pair.BaseCurrency) * volatilityScore; // dont want to use it all on one pair
                    if (baseAmount >= minTradeAmount) {
                        double adjustedPrice = lastTicker.MarketData.OrderTopSell;
                        double quoteAmount2 = baseAmount / adjustedPrice;
                        if (quoteAmount2 >= minTradeAmount) { // buy quote currency
                            // -----------------------------
                            Console.WriteLine("Attempting Buy");
                            Console.WriteLine("Price: " + adjustedPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
                            // -----------------------------
                            PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, adjustedPrice * 1.1, quoteAmount2);
                            Utility.TradeTracker.ReportBuy(pair, quoteAmount2, adjustedPrice);

                            lastTradeTime = lastTicker.Timestamp;
                            minimumSellPrice = adjustedPrice * minimumSellPriceFactor;
                            maximumPrice = adjustedPrice;
                        }
                    }
                }
            }
        }

        public void RunSimulation (params object[] parameters) {

            Console.WriteLine("Running simulation on " + pair);
            double btcBefore = Manager.GetWalletState("BTC");

            ResultSet.Variable tempVar;

            ResultSet[] meanRevResults = meanRevPredictor.GetAllResults();

            double price = 0;
            double meanRev = 0;

            bool lastBuy = false;
            double SellMinimum = 0;

            double minPrice = double.MaxValue;
            double maxPrice = double.MinValue;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(this.pair);

            for (int i = 0; i < meanRevResults.Length; i++) {

                if (meanRevResults[i].variables.TryGetValue("price", out tempVar)) price = tempVar.value;
                if (meanRevResults[i].variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;

                double score = meanRev;
                if (lastBuy && price < SellMinimum) score = 0;

                if (double.IsNaN(score) || double.IsInfinity(score)) score = 0;

                if (i % 100 == 0) Console.WriteLine("Pass: " + i + " / " + meanRevResults.Length);

                // todo: redo all this based on score and evaluate functions above

                // ------------------

                if (lastBuy) {
                    minPrice = price;
                    if (price > maxPrice) maxPrice = price;
                }
                else {
                    maxPrice = price;
                    if (price < minPrice) minPrice = price;
                }

                // ------------------
                double currPrice = tickers[i].MarketData.PriceLast;
                // ------------------
                if (score > BuyTrigger && !lastBuy) {
                    if (Buy(currPrice, tickers[i].Timestamp)) {
                        lastBuy = true;
                        SellMinimum = currPrice * minimumSellPriceFactor;
                    }
                }
                else if (score < 0 && lastBuy) {
                    if (Sell(currPrice, tickers[i].Timestamp)) {
                        lastBuy = false;
                        SellMinimum = 0;
                    }
                    else Console.WriteLine("Fail sell");
                }

                if (i % 1000 == 0) Console.WriteLine("Sim: " + i + "/" + tickers.Length);
                Thread.Sleep(0);
            }

            PoloniexBot.ClientManager.RefreshWallet();

            double btcAfter = Manager.GetWalletState("BTC");
            double netGain = btcAfter - btcBefore;

            Console.WriteLine("Done!");
            Console.WriteLine("Net Profit: " + netGain.ToString("F8") + " BTC");
        }
        private bool Buy (double price, long currTimestamp) {
            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            if (currQuoteAmount < minTradeAmount) {
                double baseAmount = Manager.GetWalletState(pair.BaseCurrency) * 0.5; // *volatilityScore; // dont want to use it all on one pair
                if (baseAmount >= minTradeAmount) {
                    double adjustedPrice = price * 1.0025;
                    double quoteAmount = baseAmount / adjustedPrice;
                    if (quoteAmount >= minTradeAmount) { // buy quote currency
                        PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, adjustedPrice, quoteAmount);
                        Utility.TradeTracker.ReportBuy(pair, quoteAmount, adjustedPrice, currTimestamp);
                        return true;
                    }
                }
            }
            return false;
        }
        private bool Sell (double price, long currTimestamp) {
            double quoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            if (quoteAmount >= minTradeAmount) {
                double adjustedPrice = price * 0.9975;
                double baseAmount = quoteAmount * adjustedPrice;
                if (baseAmount >= minTradeAmount) { // sell quote currency
                    PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, adjustedPrice, quoteAmount);
                    Utility.TradeTracker.ReportSell(pair, quoteAmount, adjustedPrice, currTimestamp);
                    return true;
                }
            }
            return false;
        }

        // base -> quote = amount / price
        // quote -> base = amount * price

        #endregion

    }
}

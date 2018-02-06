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

        private CurrencyPair pair;
        private Thread thread;

        private Strategies.Strategy strategy;

        private bool BlockedTrade = false;

        public void SetBlockedTrade (bool state) {
            this.BlockedTrade = state;

            GUI.GUIManager.BlockPair(pair, state);


        }

        #region Basic Setup
        // All executed in the main thread

        public Predictor[] GetPredictors () {
            return null;
        }

        public TPManager (CurrencyPair pair, Strategies.Strategy strategy) {
            this.pair = pair;
            invokeQueue = new TSList<InvokePair>();

            if (pair.BaseCurrency == "USDT" && pair.QuoteCurrency == "BTC") strategy = new Strategies.BaseTrendMonitor(pair);
            else this.strategy = strategy;
        }
        public TPManager (CurrencyPair pair) {
            this.pair = pair;
            invokeQueue = new TSList<InvokePair>();

            switch (pair.BaseCurrency) {
                case "USDT":
                    strategy = new Strategies.BaseTrendMonitor(pair);
                    break;
                default:
                    strategy = new Strategies.PatternMatching(pair);
                    break;
            }
        }
        public void Setup (bool pullTickerHistory = true) {

            if (pullTickerHistory) {
                Console.WriteLine("Pulling ticker history");
                Data.Store.PullTickerHistory(pair, strategy.PullTickerHistoryHours);
                Console.WriteLine("done");
            }

            strategy.Setup(pullTickerHistory);
        }
        public void RecalculateVolatility () {
            // calculate the standard price deviation in past 6 hours (in % relative to price of last ticker)

            // note: DEBUG
            strategy.SetVolatility(0.3);
            return;
            // note: DEBUG

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

            double volatilityScore = currencyUse * priceDiff * 10;
            if (volatilityScore < 0.1) volatilityScore = 0.1;
            if (volatilityScore > 0.3) volatilityScore = 0.3;

            strategy.SetVolatility(volatilityScore);
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
        public void ForceBuy () {
            strategy.ForceBuy();
        }
        public void ForceSell () {
            strategy.ForceSell();
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

                        Utility.ThreadManager.ReportAlive("TPManager ("+pair+")");
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
            RecalculateVolatility();
            strategy.UpdatePredictors();
        }
        public void EvaluateAndTrade (params object[] parameters) {
            if (!BlockedTrade) {
                strategy.EvaluateTrade();
            }
        }

        // base -> quote = amount / price
        // quote -> base = amount * price

        #endregion

        public void Reset () {
            strategy.Reset();
        }

        public Strategies.Strategy GetStrategy () {
            return strategy;
        }

    }
}

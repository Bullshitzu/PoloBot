using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class BaseTrendMonitor : Strategy {

        public BaseTrendMonitor (CurrencyPair pair) : base(pair) {
            PullTickerHistoryHours = 27;
        }

        // ------------------------------

        TradeRule ruleDelayAllTrades;
        TradeRule ruleDelayBuy;

        TradeRule ruleMinBase;
        TradeRule ruleMinBasePost;
        TradeRule ruleMinQuote;
        TradeRule ruleMinQuotePost;

        TradeRule ruleBubbleSave;

        TradeRule[] allRules = { };

        // ------------------------------

        public override void Reset () {
            base.Reset();
            Setup(true);
        }

        // ------------------------------

        public static double LastUSDTBTCPrice = 0;
        public static double LastUSDTBTCMeanRev = 0;
        public static double LastUSDTBTCADX = 0;

        // ------------------------------

        private Data.Predictors.MeanReversion predictorMeanRev;
        private Data.Predictors.ADX predictorADX;

        // ------------------------------

        public override void Setup (bool simulate = false) {

            SetupRules();

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            if (tickers.Length > 0) LastUSDTBTCPrice = tickers.Last().MarketData.PriceLast;
            Manager.UpdateWalletValue("USDT", LastUSDTBTCPrice);

            predictorMeanRev = new Data.Predictors.MeanReversion(pair, 7200);
            predictorADX = new Data.Predictors.ADX(pair);

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorMeanRev.Recalculate(tickerList.ToArray());
                predictorADX.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("BaseCurrTrendUpdater");
            }

            GUI.GUIManager.UpdateMainSummary(tickers);
        }
        private void SetupRules () {

            ruleDelayAllTrades = new RuleDelayAllTrade();
            ruleDelayBuy = new RuleDelayBuy();

            ruleForce = new RuleManualForce();

            ruleMinBase = new RuleMinimumBaseAmount();
            ruleMinBasePost = new RuleMinimumBaseAmountPost();
            ruleMinQuote = new RuleMinimumQuoteAmount();
            ruleMinQuotePost = new RuleMinimumQuoteAmountPost();

            ruleBubbleSave = new RuleBubbleSave();

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleBubbleSave, }; // buy / sell rules
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            if(tickers.Length > 0) LastUSDTBTCPrice = tickers.Last().MarketData.PriceLast;
            Manager.UpdateWalletValue("USDT", LastUSDTBTCPrice);

            predictorMeanRev.Recalculate(tickers);
            predictorADX.Recalculate(tickers);

            Utility.TradeTracker.UpdateOpenPosition(pair, tickers.Last().MarketData.OrderTopSell);

            GUI.GUIManager.UpdateMainSummary(tickers);
        }

        public override void EvaluateTrade () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currBaseAmount = Manager.GetWalletState(pair.BaseCurrency);

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currBaseAmount / sellPrice;

            // -------------------------------------------

            Data.ResultSet.Variable tempVar;
            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) LastUSDTBTCMeanRev = tempVar.value;
            if (predictorADX.GetLastResult().variables.TryGetValue("adx", out tempVar)) LastUSDTBTCADX = tempVar.value;

            if (double.IsNaN(LastUSDTBTCADX) || double.IsInfinity(LastUSDTBTCADX)) LastUSDTBTCADX = 100;

            // -------------------------------------------
            // Compile all the rule variables into a dictionary
            // -------------------------------------------

            Dictionary<string, double> ruleVariables = new Dictionary<string, double>();

            ruleVariables.Add("lastBuyTimestamp", LastBuyTime);
            ruleVariables.Add("lastSellTimestamp", LastSellTime);
            ruleVariables.Add("lastTickerTimestamp", lastTicker.Timestamp);

            ruleVariables.Add("price", lastPrice);
            ruleVariables.Add("buyPrice", buyPrice);
            ruleVariables.Add("sellPrice", sellPrice);

            ruleVariables.Add("quoteAmount", currQuoteAmount);
            ruleVariables.Add("baseAmount", currBaseAmount);
            ruleVariables.Add("baseAmountTradable", currBaseAmount);

            ruleVariables.Add("postQuoteAmount", postQuoteAmount);
            ruleVariables.Add("postBaseAmount", postBaseAmount);

            ruleVariables.Add("usdtBtcMRev", LastUSDTBTCMeanRev);
            ruleVariables.Add("usdtBtcADX", LastUSDTBTCADX);

            // -----------------------
            // Recalculate all the rules
            // -----------------------

            try {
                for (int i = 0; i < allRules.Length; i++) {
                    allRules[i].Recalculate(ruleVariables);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error: " + e.Message);
                return;
            }

            // ----------------
            // Custom rule logic
            // ----------------

            #region Buy Logic Tree
            if (ruleMinBase.Result != RuleResult.BlockBuy && ruleMinQuotePost.Result != RuleResult.BlockBuy) {
                // we have enough of base and will have enough of quote (after trade) to satisfy minimum trade amount (0.0001)
                // note: this counts the volatility factor, RuleMinimumBaseAmount uses baseAmount * volatility in verification

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell && ruleDelayBuy.Result != RuleResult.BlockBuy) {
                    // enough time has passed since the last trades were made

                    if (ruleBubbleSave.currentResult == RuleResult.Buy) {
                        // USDT / BTC price has stopped falling, so buy BTC back

                        Buy(sellPrice, postQuoteAmount, true);
                        return;
                    }
                }
            }
            #endregion

            #region Sell Logic Tree
            if (ruleMinQuote.Result != RuleResult.BlockSell && ruleMinBasePost.Result != RuleResult.BlockSell) {
                // we have enough of quote and will have enough of base (after trade) to satisfy minimum trade amount (0.0001)

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell) {
                    // enough time has passed since the last trades were made

                    if (ruleBubbleSave.currentResult == RuleResult.Sell) {
                        // USDT / BTC price is rapidly dropping, so sell BTC

                        Sell(buyPrice, currQuoteAmount, true);
                        return;
                    }
                }
            }
            #endregion
        }

        private void Buy (double sellPrice, double quoteAmount, bool modifyPrice) {

            // -----------------------------
            Console.WriteLine("Attempting Buy - " + pair);
            Console.WriteLine("Price: " + sellPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, sellPrice, quoteAmount, modifyPrice).Result;

                if (id == 0) {
                    Console.WriteLine("Error making buy");
                }
                else {
                    LastBuyTime = Data.Store.GetLastTicker(pair).Timestamp;

                    Utility.TradeTracker.ReportSell(pair, quoteAmount, sellPrice, LastBuyTime);
                    Utility.TradeTracker.SetOrderData(pair, id, sellPrice);

                    GUI.GUIManager.SetMainMarked(false);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making buy: " + e.Message);
            }
        }
        private void Sell (double buyPrice, double quoteAmount, bool modifyPrice) {

            // -----------------------------
            Console.WriteLine("Attempting Sell - " + pair);
            Console.WriteLine("Price: " + buyPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, buyPrice, quoteAmount, modifyPrice).Result;

                if (id == 0) {
                    Console.WriteLine("Error making sale");
                }
                else {
                    LastSellTime = Data.Store.GetLastTicker(pair).Timestamp;

                    Utility.TradeTracker.ReportBuy(pair, quoteAmount, buyPrice, LastSellTime);
                    Utility.TradeTracker.SetOrderData(pair, id, buyPrice);

                    GUI.GUIManager.SetMainMarked(true);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message);
            }
        }
    }
}

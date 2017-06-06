using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class LowAlts : Strategy {

        public LowAlts (CurrencyPair pair) : base(pair) { }

        // ------------------------------

        TradeRule ruleDelayAllTrades;
        TradeRule ruleDelayBuy;

        TradeRule ruleMinBase;
        TradeRule ruleMinBasePost;
        TradeRule ruleMinQuote;
        TradeRule ruleMinQuotePost;

        TradeRule ruleMinSellprice;
        TradeRule ruleSellBand;
        TradeRule ruleStopLoss;

        TradeRule ruleMacd;
        TradeRule ruleMeanRev;

        TradeRule[] allRules = { };

        // ------------------------------

        private double openPosition = 0;
        private double maximumPrice = 0;

        private Data.Predictors.MACD predictorMacd;
        private Data.Predictors.MeanReversion predictorMeanRev;

        public override void Setup () {

            // ----------------------------------

            ruleDelayAllTrades = new RuleDelayAllTrade();
            ruleDelayBuy = new RuleDelayBuy();

            ruleForce = new RuleManualForce();

            ruleMinBase = new RuleMinimumBaseAmount();
            ruleMinBasePost = new RuleMinimumBaseAmountPost();
            ruleMinQuote = new RuleMinimumQuoteAmount();
            ruleMinQuotePost = new RuleMinimumQuoteAmountPost();

            ruleMinSellprice = new RuleMinimumSellPrice();
            ruleSellBand = new RuleSellBand();
            ruleStopLoss = new RuleStopLoss();

            ruleMacd = new RuleMACD();
            ruleMeanRev = new RuleMeanRev();

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss,  // sell rules
                ruleMacd, ruleMeanRev }; // buy rules

            // ----------------------------------

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;
            maximumPrice = openPos;

            predictorMacd = new Data.Predictors.MACD(pair);
            predictorMeanRev = new Data.Predictors.MeanReversion(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorMacd.Recalculate(tickerList.ToArray());
                predictorMeanRev.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("LowAlts");
            }
        }
        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorMacd.Recalculate(tickers);
            predictorMeanRev.Recalculate(tickers);

            if (buyPrice > maximumPrice) maximumPrice = buyPrice;
            Utility.TradeTracker.UpdateOpenPosition(pair, buyPrice);
        }
        public override void EvaluateTrade () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currBaseAmount = Manager.GetWalletState(pair.BaseCurrency);

            double currTradableBaseAmount = currBaseAmount * 0.3; // VolatilityScore;

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currTradableBaseAmount / sellPrice;

            double macd = 0;
            double meanRev = 0;

            Data.ResultSet.Variable tempVar;
            if (predictorMacd.GetLastResult().variables.TryGetValue("macdHistogramAdjusted", out tempVar)) macd = tempVar.value;
            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;

            // -------------------------------
            // Update the trade history screen
            // -------------------------------

            // todo: update trade history

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

            ruleVariables.Add("openPrice", openPosition);
            ruleVariables.Add("maximumPrice", maximumPrice);

            ruleVariables.Add("quoteAmount", currQuoteAmount);
            ruleVariables.Add("baseAmount", currBaseAmount);

            ruleVariables.Add("baseAmountTradable", currTradableBaseAmount);

            ruleVariables.Add("postQuoteAmount", postQuoteAmount);
            ruleVariables.Add("postBaseAmount", postBaseAmount);

            ruleVariables.Add("macd", macd);
            ruleVariables.Add("meanRev", meanRev);

            PoloniexBot.Windows.GUIManager.strategyWindow.strategyScreen.UpdateData(pair, ruleVariables);

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

                if (ruleForce.Result == RuleResult.Buy) {
                    Buy(sellPrice, postQuoteAmount);
                    return;
                }

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell && ruleDelayBuy.Result != RuleResult.BlockBuy) {
                    // enough time has passed since the last trades were made

                    if (ruleMinQuote.Result == RuleResult.BlockSell) {
                        // if it's blocking sell that means we don't own quote, so go ahead with buying

                        if (ruleMacd.Result == RuleResult.Buy && ruleMeanRev.Result == RuleResult.Buy) {
                            // Price is below average and stopped declining

                            Buy(sellPrice, postQuoteAmount);
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Sell Logic Tree
            if (ruleMinQuote.Result != RuleResult.BlockSell && ruleMinBasePost.Result != RuleResult.BlockSell) {
                // we have enough of quote and will have enough of base (after trade) to satisfy minimum trade amount (0.0001)

                if (ruleForce.Result == RuleResult.Sell) {
                    Sell(buyPrice, currQuoteAmount);
                    return;
                }

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell) {
                    // enough time has passed since the last trades were made

                    if (ruleMinSellprice.Result != RuleResult.BlockSell && ruleSellBand.Result == RuleResult.Sell) {
                        // current price is profitable and is below the sell band
                        Sell(buyPrice, currQuoteAmount);
                        return;
                    }

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // the price has dropped 10% since buying
                        // sell to prevent further losses
                        Sell(buyPrice, currQuoteAmount);
                        return;
                    }
                }
            }
            #endregion
        }

        private void Buy (double sellPrice, double quoteAmount) {

            // -----------------------------
            Console.WriteLine("Attempting Buy - " + pair);
            Console.WriteLine("Price: " + sellPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, sellPrice, quoteAmount).Result;

            if (id == 0) {
                Console.WriteLine("Error making buy");
            }
            else {
                Utility.TradeTracker.ReportBuy(pair, quoteAmount, sellPrice);

                LastBuyTime = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);

                openPosition = sellPrice;
                maximumPrice = sellPrice;

                ruleForce.currentResult = RuleResult.None;
            }
        }

        private void Sell (double buyPrice, double quoteAmount) {

            // -----------------------------
            Console.WriteLine("Attempting Sell - " + pair);
            Console.WriteLine("Price: " + buyPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            Task<ulong> postOrderTask = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, buyPrice, quoteAmount);
            ulong id = postOrderTask.Result;

            if (id == 0) {
                Console.WriteLine("Error making sale");
            }
            else {
                Utility.TradeTracker.ReportSell(pair, quoteAmount, buyPrice);
                
                LastSellTime = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);

                openPosition = 0;
                maximumPrice = 0;

                ruleForce.currentResult = RuleResult.None;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class PriceTrend : Strategy {

        public PriceTrend (CurrencyPair pair) : base(pair) {
            Windows.Controls.StrategyScreen.drawVariables = new string[] { "meanRev", "macdTrend" };
        }

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

        TradeRule rulePriceDelta;
        TradeRule ruleMeanRev;

        TradeRule[] allRules = { };

        RuleGlobalDrop ruleGlobalTrend;

        // ------------------------------

        private double openPosition = 0;

        private Data.Predictors.PriceExtremes predictorExtremes;
        private Data.Predictors.MeanReversion predictorMeanRev;
        private Data.Predictors.PriceDelta predictorPriceDelta;

        public override void Setup (bool simulated = false) {

            // ----------------------------------

            SetupRules();

            // ----------------------------------

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorMeanRev = new Data.Predictors.MeanReversion(pair);
            predictorPriceDelta = new Data.Predictors.PriceDelta(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorMeanRev.Recalculate(tickerList.ToArray());
                predictorPriceDelta.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("PriceTrend");
            }
        }
        private void SetupRules () {
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

            rulePriceDelta = new RulePriceDelta();
            ruleMeanRev = new RuleMeanRev();

            ruleGlobalTrend = new RuleGlobalDrop("meanRev");

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss,  // sell rules
                rulePriceDelta, ruleMeanRev }; // buy rules
        }

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorPriceDelta = null;
            predictorMeanRev = null;

            Setup(true);
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorExtremes.Update(tickers);
            predictorMeanRev.Recalculate(tickers);
            predictorPriceDelta.Recalculate(tickers);

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

            double meanRev = 0;
            double minPrice = 0;
            double maxPrice = 0;
            
            double priceDelta1 = 0;
            double priceDelta2 = 0;
            double priceDelta3 = 0;

            Data.ResultSet.Variable tempVar;
            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;
            if (predictorPriceDelta.GetLastResult().variables.TryGetValue("priceDelta1", out tempVar)) priceDelta1 = tempVar.value;
            if (predictorPriceDelta.GetLastResult().variables.TryGetValue("priceDelta2", out tempVar)) priceDelta2 = tempVar.value;
            if (predictorPriceDelta.GetLastResult().variables.TryGetValue("priceDelta3", out tempVar)) priceDelta3 = tempVar.value;
            
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

            ruleVariables.Add("quoteAmount", currQuoteAmount);
            ruleVariables.Add("baseAmount", currBaseAmount);

            ruleVariables.Add("baseAmountTradable", currTradableBaseAmount);

            ruleVariables.Add("postQuoteAmount", postQuoteAmount);
            ruleVariables.Add("postBaseAmount", postBaseAmount);

            ruleVariables.Add("meanRev", meanRev);

            ruleVariables.Add("minPrice", minPrice);
            ruleVariables.Add("maxPrice", maxPrice);

            ruleVariables.Add("minGUI", lastPrice / minPrice);
            ruleVariables.Add("maxGUI", maxPrice / lastPrice);

            ruleVariables.Add("priceDelta1", priceDelta1);
            ruleVariables.Add("priceDelta2", priceDelta2);
            ruleVariables.Add("priceDelta3", priceDelta3);
            
            // -----------------------
            // Recalculate global rules
            // -----------------------

            ruleGlobalTrend.Recalculate(ruleVariables, pair);
            ruleVariables.Add("mRevGlobal", RuleGlobalDrop.GetGlobalTrend());

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
            // Update GUI
            // ----------------

            PoloniexBot.Windows.GUIManager.strategyWindow.strategyScreen.UpdateData(pair, ruleVariables);

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

                        if (ruleGlobalTrend.Result == RuleResult.None) {
                            // global trend isn't dropping

                            if (rulePriceDelta.currentResult == RuleResult.Buy) { //  && ruleMeanRev.currentResult == RuleResult.Buy
                                // Price has stopped falling and is below average

                                Buy(sellPrice, postQuoteAmount);
                                return;
                            }
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

                    if (ruleMinSellprice.Result != RuleResult.BlockSell) {
                        // current price is profitable

                        if (ruleSellBand.Result == RuleResult.Sell) {
                            // price is below the sell band

                            Sell(buyPrice, currQuoteAmount);
                            return;
                        }
                    }

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // price has dropped below stop-loss

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

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, sellPrice, quoteAmount).Result;

                if (id == 0) {
                    Console.WriteLine("Error making buy");
                }
                else {
                    Utility.TradeTracker.ReportBuy(pair, quoteAmount, sellPrice);

                    LastBuyTime = Data.Store.GetLastTicker(pair).Timestamp;

                    openPosition = sellPrice;
                    predictorExtremes.CurrentMaximum = sellPrice;

                    ruleForce.currentResult = RuleResult.None;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making buy: " + e.Message);
            }
        }
        private void Sell (double buyPrice, double quoteAmount) {

            // -----------------------------
            Console.WriteLine("Attempting Sell - " + pair);
            Console.WriteLine("Price: " + buyPrice.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, buyPrice, quoteAmount).Result;

                if (id == 0) {
                    Console.WriteLine("Error making sale");
                }
                else {
                    Utility.TradeTracker.ReportSell(pair, quoteAmount, buyPrice);

                    LastSellTime = Data.Store.GetLastTicker(pair).Timestamp;

                    openPosition = 0;
                    predictorExtremes.CurrentMaximum = 0;
                    predictorExtremes.CurrentMinimum = buyPrice;

                    ruleForce.currentResult = RuleResult.None;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class PatternMatching : Strategy {

        public PatternMatching (CurrencyPair pair) : base(pair) {
            if (pair.BaseCurrency == "USDT") PullTickerHistoryHours = 27;
        }

        // ------------------------------

        TradeRule ruleDelayAllTrades;
        TradeRule ruleDelayBuy;

        TradeRule ruleMinBase;
        TradeRule ruleMinBasePost;
        TradeRule ruleMinQuote;
        TradeRule ruleMinQuotePost;

        TradeRule ruleMinQuoteOrders;

        TradeRule ruleMinSellprice;
        TradeRule ruleSellBand;
        TradeRule ruleStopLoss;

        TradeRule rulePatternMatch;

        TradeRule ruleMinSellPriceDump;
        TradeRule ruleDump;

        TradeRule[] allRules = { };

        // ------------------------------

        private ulong orderID = 0;
        private double orderPrice = 0;

        private double openPosition = 0;

        private Data.Predictors.PriceExtremes predictorExtremes;
        private Data.Predictors.PatternMatch predictorPatternMatch;

        // ------------------------------

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorPatternMatch = null;

            Setup(true);
        }

        public override void Setup (bool simulate = false) {

            GUI.GUIManager.AddStrategyScreenPair(this.pair);

            // ----------------------------------

            SetupRules();

            // ----------------------------------

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorPatternMatch = new Data.Predictors.PatternMatch(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);

            List<TickerChangedEventArgs> currTickers = new List<TickerChangedEventArgs>();
            currTickers.AddRange(tickers.Take(tickers.Length - 20));

            for (int i = tickers.Length - 20; i < tickers.Length; i++) {
                currTickers.Add(tickers[i]);
                predictorPatternMatch.Recalculate(currTickers.ToArray());
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

            ruleMinQuoteOrders = new RuleMinimumQuoteAmountOrders();

            ruleMinSellprice = new RuleMinimumSellPrice();
            ruleSellBand = new RuleSellBand();
            ruleStopLoss = new RuleStopLoss(0.85);

            ruleMinSellPriceDump = new RuleMinimumSellPriceDump();
            ruleDump = new RuleDump();

            rulePatternMatch = new RulePatternMatch(2);

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinQuoteOrders, // minimum orders amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss,  // sell rules
                ruleMinSellPriceDump, ruleDump, // dump rules
                rulePatternMatch }; // buy rules
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorExtremes.Update(tickers);
            predictorPatternMatch.Recalculate(tickers);

            GUI.GUIManager.SetPairSummary(this.pair, tickers, lastTicker.MarketData.Volume24HourBase);

            Utility.TradeTracker.UpdateOpenPosition(pair, buyPrice);
        }

        public override void EvaluateTrade () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currBaseAmount = Manager.GetWalletState(pair.BaseCurrency);

            double currQuoteOrdersAmount = Manager.GetWalletStateOrders(pair.QuoteCurrency);

            double currTradableBaseAmount = currBaseAmount;
            if (currTradableBaseAmount < RuleMinimumBaseAmount.MinimumTradeAmount) currTradableBaseAmount = currBaseAmount;

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currTradableBaseAmount / sellPrice;

            double patternMatchResult = 0;
            double minPrice = 0;
            double maxPrice = 0;

            // --------------------------

            Data.ResultSet.Variable tempVar;

            if (predictorPatternMatch.GetLastResult().variables.TryGetValue("result", out tempVar)) patternMatchResult = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;

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

            ruleVariables.Add("quoteAmountOrders", currQuoteOrdersAmount);

            ruleVariables.Add("quoteAmount", currQuoteAmount);
            ruleVariables.Add("baseAmount", currBaseAmount);

            ruleVariables.Add("baseAmountTradable", currTradableBaseAmount);

            ruleVariables.Add("postQuoteAmount", postQuoteAmount);
            ruleVariables.Add("postBaseAmount", postBaseAmount);

            ruleVariables.Add("patternMatchResult", patternMatchResult);

            ruleVariables.Add("minPrice", minPrice);
            ruleVariables.Add("maxPrice", maxPrice);

            ruleVariables.Add("minGUI", lastPrice / minPrice);
            ruleVariables.Add("maxGUI", maxPrice / lastPrice);

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

            GUI.GUIManager.UpdateStrategyScreenPair(this.pair, ruleVariables);

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

                        if (rulePatternMatch.Result == RuleResult.Buy) {
                            // current pattern indicates the price will rise

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

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // price has dropped below stop-loss

                        Sell(buyPrice, currQuoteAmount);
                        return;
                    }

                    if (ruleMinSellPriceDump.Result != RuleResult.BlockSell) {
                        // current price is profitable (0.5%)

                        if (ruleDump.Result == RuleResult.Sell) {
                            // long time has passed since buy, price is now unpredictable so sell asap

                            Sell(buyPrice, currQuoteAmount);
                            return;
                        }
                    }

                    if (ruleMinSellprice.Result != RuleResult.BlockSell) {
                        // current price is profitable

                        if (ruleSellBand.Result == RuleResult.Sell) {
                            // price is below the sell band

                            Sell(buyPrice, currQuoteAmount);
                            return;
                        }
                    }

                    if (rulePatternMatch.Result == RuleResult.Sell) {
                        // current pattern indicates the price will fall

                        Sell(buyPrice, currQuoteAmount);
                        return;
                    }
                }
            }
            #endregion

        }

        // ---------------------

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
                    LastBuyTime = Data.Store.GetLastTicker(pair).Timestamp;

                    this.orderID = id;
                    this.orderPrice = sellPrice;

                    openPosition = sellPrice;
                    predictorExtremes.CurrentMaximum = sellPrice;
                    predictorExtremes.CurrentMinimum = sellPrice;

                    ruleForce.currentResult = RuleResult.None;

                    Utility.TradeTracker.ReportBuy(pair, quoteAmount, sellPrice, LastBuyTime);
                    Utility.TradeTracker.SetOrderData(pair, id, sellPrice);
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
                    LastSellTime = Data.Store.GetLastTicker(pair).Timestamp;

                    this.orderID = id;
                    this.orderPrice = buyPrice;

                    ruleForce.currentResult = RuleResult.None;

                    Utility.TradeTracker.ReportSell(pair, quoteAmount, buyPrice, LastSellTime);
                    Utility.TradeTracker.SetOrderData(pair, id, buyPrice);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message);
            }
        }
    }
}

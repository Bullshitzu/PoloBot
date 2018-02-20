using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class BizStrat : Strategy {

        public BizStrat (CurrencyPair pair) : base(pair) { }

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
        RuleStopLoss ruleStopLoss;

        TradeRule ruleMinSellPriceDump;
        TradeRule ruleDump;

        TradeRule ruleRSI;
        TradeRule ruleMacdConverge;

        TradeRule[] allRules = { };

        // ------------------------------

        private double openPosition = 0;

        private double lastTradePrice = 0;

        private Data.Predictors.PriceExtremes predictorExtremes;

        private Data.Predictors.MACD predictorMacd;
        private Data.Predictors.RSI predictorRSI;

        // ------------------------------

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorMacd = null;
            predictorRSI = null;

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

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            if (lastTicker == null) throw new Exception("Couldn't build timeframe model for " + pair + " - no tickers available");

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorMacd = new Data.Predictors.MACD(pair, 3600);
            predictorRSI = new Data.Predictors.RSI(pair, 3600);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);
            
            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorMacd.Recalculate(tickerList.ToArray());
                predictorRSI.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("MeanRevADX");
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
            ruleStopLoss = new RuleStopLoss(0.9);

            ruleMinSellPriceDump = new RuleMinimumSellPriceDump();
            ruleDump = new RuleDump();

            ruleMacdConverge = new RuleMACD();
            ruleRSI = new RuleRSI(40, 70);

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinQuoteOrders, // minimum orders amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss, // sell rules
                ruleMinSellPriceDump, ruleDump, // dump rules
                ruleMacdConverge, ruleRSI }; // buy rules
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorExtremes.Update(tickers);
            predictorMacd.Recalculate(tickers);
            predictorRSI.Recalculate(tickers);

            Utility.TradeTracker.UpdateOpenPosition(pair, buyPrice);

            GUI.GUIManager.SetPairSummary(this.pair, tickers, lastTicker.MarketData.Volume24HourBase);
        }

        public override void EvaluateTrade () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            if (lastPrice == lastTradePrice) return;
            lastTradePrice = lastPrice;

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currBaseAmount = Manager.GetWalletState(pair.BaseCurrency);

            double currQuoteOrdersAmount = Manager.GetWalletStateOrders(pair.QuoteCurrency);

            double currTradableBaseAmount = currBaseAmount * 1;
            if (currTradableBaseAmount < RuleMinimumBaseAmount.MinimumTradeAmount) currTradableBaseAmount = currBaseAmount;

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currTradableBaseAmount / sellPrice;

            double minPrice = 0;
            double maxPrice = 0;
            double maShort = 0;
            double maLong = 0;
            double rsi = 0;

            // --------------------------

            Data.ResultSet.Variable tempVar;
            if (predictorRSI.GetLastResult().variables.TryGetValue("rsi", out tempVar)) rsi = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;
            if (predictorMacd.GetLastResult().variables.TryGetValue("maShort", out tempVar)) maShort = tempVar.value;
            if (predictorMacd.GetLastResult().variables.TryGetValue("maLong", out tempVar)) maLong = tempVar.value;

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

            ruleVariables.Add("maShort", maShort);
            ruleVariables.Add("maLong", maLong);
            ruleVariables.Add("rsi", rsi);

            ruleVariables.Add("minPrice", minPrice);
            ruleVariables.Add("maxPrice", maxPrice);

            ruleVariables.Add("minGUI", lastPrice / minPrice);
            ruleVariables.Add("maxGUI", maxPrice / lastPrice);

            // -----------------------
            // Update the sell band price rise offset
            // -----------------------

            ((RuleSellBand)ruleSellBand).PriceRiseOffset = predictorExtremes.PriceRiseOffset;

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

            Utility.TradeTracker.UpdateStopLoss(pair, ruleStopLoss.currTrigger);

            // ----------------
            // Custom rule logic
            // ----------------

            #region Buy Logic Tree
            if (ruleMinBase.Result != RuleResult.BlockBuy && ruleMinQuotePost.Result != RuleResult.BlockBuy) {
                // we have enough of base and will have enough of quote (after trade) to satisfy minimum trade amount (0.0001)
                // note: this counts the volatility factor, RuleMinimumBaseAmount uses baseAmount * volatility in verification

                if (ruleForce.Result == RuleResult.Buy) {
                    Buy(sellPrice, postQuoteAmount, true);
                    return;
                }

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell && ruleDelayBuy.Result != RuleResult.BlockBuy && !RuleBubbleSave.BlockTrade) {
                    // enough time has passed since the last trades were made

                    if (ruleMinQuote.Result == RuleResult.BlockSell) {
                        // if it's blocking sell that means we don't own quote, so go ahead with buying

                        if (ruleMacdConverge.Result == RuleResult.Buy && ruleRSI.Result == RuleResult.Buy) {
                            Buy(sellPrice, postQuoteAmount, true);
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
                    Sell(buyPrice, currQuoteAmount, true);
                    return;
                }

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell) {
                    // enough time has passed since the last trades were made

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // price has dropped below stop-loss

                        Sell(buyPrice, currQuoteAmount, true);
                        return;
                    }

                    if (ruleMinSellprice.Result != RuleResult.BlockSell) {
                        // current price is profitable (2%)

                        if (ruleSellBand.Result == RuleResult.Sell) {
                            // price is below the sell band

                            Sell(buyPrice, currQuoteAmount, false);
                            return;
                        }
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

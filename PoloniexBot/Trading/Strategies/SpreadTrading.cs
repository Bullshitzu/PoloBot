using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class SpreadTrading : Strategy {

        public SpreadTrading (CurrencyPair pair) : base(pair) {
        }

        // ------------------------------

        TradeRule ruleDelayAllTrades;
        TradeRule ruleDelayBuy;

        TradeRule ruleMinBase;
        TradeRule ruleMinBasePost;
        TradeRule ruleMinQuote;
        TradeRule ruleMinQuotePost;

        TradeRule ruleMinBaseOrders;
        TradeRule ruleMinQuoteOrders;

        TradeRule ruleMinSellprice;
        TradeRule ruleStopLoss;

        TradeRule rulePriceDelta;

        TradeRule[] allRules = { };

        // ------------------------------

        private ulong orderID = 0;
        private double orderPrice = 0;

        // ------------------------------

        private double openPosition = 0;

        private Data.Predictors.PriceExtremes predictorExtremes;
        private Data.Predictors.DirectionIndex predictorDX;

        // ------------------------------

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorDX = null;

            Setup(true);
        }

        public override void Setup (bool simulate = false) {

            GUI.GUIManager.AddStrategyScreenPair(this.pair);

            // ----------------------------------

            double optTrigger = double.MaxValue;

            try {
                Data.VarAnalysis.VarPairData vpd = Data.VarAnalysis.LoadResults(pair);
                optTrigger = vpd.deltaValue;

                Console.WriteLine(pair + ": " + optTrigger.ToString("F8"));

                // todo: check vpd timestamp
                // (if older then 24h then it needs refreshing)
            }
            catch (Exception e) {
                CLI.Manager.PrintWarning("No optimized pair data for " + pair + "!");
            }

            optTrigger *= 15;

            // ----------------------------------

            SetupRules(optTrigger);

            // ----------------------------------

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            orderID = Utility.TradeTracker.GetOrderID(pair);
            orderPrice = Utility.TradeTracker.GetOrderPrice(pair);

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorDX = new Data.Predictors.DirectionIndex(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorDX.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("LowAlts");
            }

        }
        private void SetupRules (double optTrigger) {
            ruleDelayAllTrades = new RuleDelayAllTrade();
            ruleDelayBuy = new RuleDelayBuy();

            ruleForce = new RuleManualForce();

            ruleMinBase = new RuleMinimumBaseAmount();
            ruleMinBasePost = new RuleMinimumBaseAmountPost();
            ruleMinQuote = new RuleMinimumQuoteAmount();
            ruleMinQuotePost = new RuleMinimumQuoteAmountPost();

            ruleMinBaseOrders = new RuleMinimumBaseAmountOrders();
            ruleMinQuoteOrders = new RuleMinimumQuoteAmountOrders();

            ruleMinSellprice = new RuleMinimumSellPriceGiver();
            ruleStopLoss = new RuleStopLossSpread(0.9);

            rulePriceDelta = new RulePriceDelta(optTrigger);

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinBaseOrders, ruleMinQuoteOrders, // minimum amounts in orders
                ruleMinSellprice, ruleStopLoss,  // sell rules
                rulePriceDelta }; // buy rules
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorExtremes.Update(tickers);
            predictorDX.Recalculate(tickers);

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
            double currBaseOrdersAmount = Manager.GetWalletStateOrders(pair.BaseCurrency);

            double currTradableBaseAmount = currBaseAmount * 0.5;
            if (currTradableBaseAmount < RuleMinimumBaseAmount.MinimumAllowedTradeAmount) currTradableBaseAmount = currBaseAmount;

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currTradableBaseAmount / sellPrice;

            double minPrice = 0;
            double maxPrice = 0;

            double delta1 = 0;

            // --------------------------

            Data.ResultSet.Variable tempVar;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;

            if (predictorDX.GetLastResult().variables.TryGetValue("delta1", out tempVar)) delta1 = tempVar.value;

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

            ruleVariables.Add("quoteAmountOrders", currQuoteOrdersAmount);
            ruleVariables.Add("baseAmountOrders", currBaseOrdersAmount);

            ruleVariables.Add("baseAmountTradable", currTradableBaseAmount);

            ruleVariables.Add("postQuoteAmount", postQuoteAmount);
            ruleVariables.Add("postBaseAmount", postBaseAmount);

            ruleVariables.Add("priceDelta1", delta1);

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

            if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell) {

                // look at base order amount (phase 2: buying)
                if (ruleMinBaseOrders.Result != RuleResult.BlockBuy) {
                    // update the buy order

                    if (buyPrice > orderPrice) {
                        // price is higher then my buy price, so move mine to the top

                        Move(orderID, buyPrice + Utility.Constants.OneSatoshi, true);
                        return;
                    }

                    return;
                }

                // look at quote amount (phase 3: need to sell)
                if (ruleMinQuote.Result != RuleResult.BlockSell && ruleMinBasePost.Result != RuleResult.BlockSell) {
                    // we have enough of quote and will have enough of base (after trade) to satisfy minimum trade amount (0.0001)

                    Sell(openPosition * 1.015, currQuoteAmount);
                    return;
                }

                // look at base amount (phase 1: need to buy)
                if (ruleMinBase.Result != RuleResult.BlockBuy && ruleMinQuotePost.Result != RuleResult.BlockBuy) {
                    // we have enough of base and will have enough of quote (after trade) to satisfy minimum trade amount (0.0001)
                    // note: this counts the volatility factor, RuleMinimumBaseAmount uses baseAmount * volatility in verification

                    if (ruleForce.Result == RuleResult.Buy) {
                        Buy(buyPrice + Utility.Constants.OneSatoshi, postQuoteAmount);
                        return;
                    }
                    
                    if (rulePriceDelta.currentResult == RuleResult.Buy) {
                        // price has stopped falling and is below average

                        Buy(buyPrice + Utility.Constants.OneSatoshi, postQuoteAmount);
                        return;
                    }

                    return;
                }
            }
        }

        private void Buy (double price, double quoteAmount) {

            // -----------------------------
            Console.WriteLine("Attempting Buy - " + pair);
            Console.WriteLine("Price: " + price.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, price, quoteAmount, false).Result;

                if (id == 0) {
                    Console.WriteLine("Error making buy");
                }
                else {
                    LastBuyTime = Data.Store.GetLastTicker(pair).Timestamp;

                    orderID = id;
                    orderPrice = price;

                    openPosition = price;
                    predictorExtremes.CurrentMaximum = price;
                    predictorExtremes.CurrentMinimum = price;

                    ruleForce.currentResult = RuleResult.None;

                    Utility.TradeTracker.ReportBuy(pair, quoteAmount, price, LastBuyTime);
                    Utility.TradeTracker.SetOrderData(pair, id, price);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making buy: " + e.Message);
            }
        }
        private void Sell (double price, double quoteAmount) {

            // -----------------------------
            Console.WriteLine("Attempting Sell - " + pair);
            Console.WriteLine("Price: " + price.ToString("F8") + ", Amount: " + quoteAmount.ToString("F8"));
            // -----------------------------

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, price, quoteAmount, false).Result;

                if (id == 0) {
                    Console.WriteLine("Error making sale");
                }
                else {
                    LastSellTime = Data.Store.GetLastTicker(pair).Timestamp;

                    orderID = id;
                    orderPrice = price;

                    ruleForce.currentResult = RuleResult.None;

                    Utility.TradeTracker.ReportSell(pair, price, price, LastSellTime);
                    Utility.TradeTracker.SetOrderData(pair, id, price);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message);
            }
        }
        private void Move (ulong orderID, double price, bool isBuy = false) {
            if (orderID == 0) return;

            // -----------------------------
            Console.WriteLine("Attempting Move - " + orderID);
            Console.WriteLine("Price: " + price.ToString("F8"));
            // -----------------------------

            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.MoveOrderAsync(orderID, price).Result;

                if (id == 0) {
                    Console.WriteLine("Error moving order");
                }
                else {
                    orderID = id;
                    orderPrice = price;

                    if (isBuy) {
                        LastBuyTime = Data.Store.GetLastTicker(pair).Timestamp;

                        openPosition = price;
                        predictorExtremes.CurrentMaximum = price;
                        predictorExtremes.CurrentMinimum = price;
                    }
                    else {
                        LastSellTime = Data.Store.GetLastTicker(pair).Timestamp;
                    }

                    Utility.TradeTracker.SetOrderData(pair, id, price);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error moving order: " + e.Message);
            }
        }
    }
}

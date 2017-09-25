using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class MeanRevDX : Strategy {

        public MeanRevDX (CurrencyPair pair) : base(pair) {
            Windows.Controls.StrategyScreen.drawVariables = new string[] { "priceDelta1", "priceDelta2", "priceDelta3" };
            Windows.Controls.StrategyScreen.minVariables = new double[] {
                RulePriceDelta.Trigger1 - 2,
                RulePriceDelta.Trigger2 - 2,
                RulePriceDelta.Trigger3 - 2,
            };
            Windows.Controls.StrategyScreen.maxVariables = new double[] {
                RulePriceDelta.Trigger1 + 2,
                RulePriceDelta.Trigger2 + 2,
                RulePriceDelta.Trigger3 + 2,
            };
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
        TradeRule ruleCh24;

        TradeRule[] allRules = { };

        // ------------------------------

        private double openPosition = 0;
        double optTrigger = 0.5;

        private Data.Predictors.PriceExtremes predictorExtremes;
        private Data.Predictors.PriceDelta predictorPriceDelta;
        private Data.Predictors.DirectionIndex predictorDX;

        // ------------------------------

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorPriceDelta = null;
            predictorDX = null;

            Setup(true);
        }

        public override void Setup (bool simulate = false) {

            PoloniexBot.Windows.GUIManager.strategyWindow.strategyScreen.UpdateData(pair, null);

            // ----------------------------------

            try {
                Data.VarAnalysis.VarPairData vpd = Data.VarAnalysis.LoadResults(pair);
                optTrigger = vpd.deltaValue;

                Console.WriteLine(pair+": "+optTrigger.ToString("F8"));

                // todo: check vpd timestamp
                // (if older then 24h then it needs refreshing)
            }
            catch (Exception e) {
                CLI.Manager.PrintWarning("No optimized pair data for " + pair + "!");
            }

            optTrigger *= 50;

            // ----------------------------------

            SetupRules(optTrigger);

            // ----------------------------------

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorPriceDelta = new Data.Predictors.PriceDelta(pair, 86400);
            predictorDX = new Data.Predictors.DirectionIndex(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);
            predictorPriceDelta.Recalculate(tickers);

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

            ruleMinSellprice = new RuleMinimumSellPrice();
            ruleSellBand = new RuleSellBand();
            ruleStopLoss = new RuleStopLoss();

            rulePriceDelta = new RulePriceDelta(optTrigger);
            ruleCh24 = new RuleCh24();

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss,  // sell rules
                rulePriceDelta, ruleCh24 }; // buy rules
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorExtremes.Update(tickers);
            predictorPriceDelta.Recalculate(tickers);
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

            double currTradableBaseAmount = currBaseAmount * 0.3; // VolatilityScore;

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currTradableBaseAmount / sellPrice;

            double minPrice = 0;
            double maxPrice = 0;

            double delta1 = 0;
            double delta2 = 0;
            double delta3 = 0;

            double ch24 = 0;

            // --------------------------

            Data.ResultSet.Variable tempVar;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;

            if (predictorPriceDelta.GetLastResult().variables.TryGetValue("priceDelta", out tempVar)) ch24 = tempVar.value;

            if (predictorDX.GetLastResult().variables.TryGetValue("delta1", out tempVar)) delta1 = tempVar.value;
            if (predictorDX.GetLastResult().variables.TryGetValue("delta2", out tempVar)) delta2 = tempVar.value;
            if (predictorDX.GetLastResult().variables.TryGetValue("delta3", out tempVar)) delta3 = tempVar.value;

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

            ruleVariables.Add("priceDelta1", delta1);
            ruleVariables.Add("priceDelta2", delta2);
            ruleVariables.Add("priceDelta3", delta3);

            ruleVariables.Add("ch24", ch24);

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

                        if (rulePriceDelta.currentResult == RuleResult.Buy && ruleCh24.currentResult == RuleResult.Buy) {
                            // price has stopped falling and is below average

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

                    if (ruleMinSellprice.Result != RuleResult.BlockSell) {
                        // current price is profitable

                        if (ruleSellBand.Result == RuleResult.Sell) {
                            // price is below the sell band

                            Sell(buyPrice, currQuoteAmount);
                            SaveTradeData(true, LastSellTime - LastBuyTime);
                            return;
                        }
                    }

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // price has dropped below stop-loss

                        Sell(buyPrice, currQuoteAmount);
                        SaveTradeData(false, LastSellTime - LastBuyTime);
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
                    LastBuyTime = Data.Store.GetLastTicker(pair).Timestamp;

                    Utility.TradeTracker.ReportBuy(pair, quoteAmount, sellPrice, LastBuyTime);

                    openPosition = sellPrice;
                    predictorExtremes.CurrentMaximum = sellPrice;
                    predictorExtremes.CurrentMinimum = sellPrice;

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
                    LastSellTime = Data.Store.GetLastTicker(pair).Timestamp;

                    Utility.TradeTracker.ReportSell(pair, quoteAmount, buyPrice, LastSellTime);

                    ruleForce.currentResult = RuleResult.None;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message);
            }
        }

        // ---------------------------------------------

        private void SaveTradeData (bool profit, long timespan) {

            List<string> lines = new List<string>();

            int hours = (int)(timespan / 3600);
            int minutes = (int)((timespan % 3600) / 60);
            int seconds = (int)(timespan % 60);

            double minPercent = ((predictorExtremes.CurrentMinimum - predictorExtremes.CurrentPrice) / predictorExtremes.CurrentPrice) * 100;

            lines.Add("");
            lines.Add(pair.ToString());
            lines.Add(hours + ":" + minutes + ":" + seconds);
            lines.Add("Minimum: " + minPercent.ToString("F4") + "%");
            lines.Add("");

            string filename = "Logs/Trades" + (profit ? "Good" : "Bad") + ".data";

            Utility.FileManager.SaveFileConcat(filename, lines.ToArray());

        }
    }
}

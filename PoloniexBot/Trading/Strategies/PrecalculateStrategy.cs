using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexBot.Trading.Rules;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    class PrecalculateStrategy {

        public PrecalculateStrategy (CurrencyPair pair) {
            this.pair = pair;
        }

        public CurrencyPair pair;

        // ---------------------------------------

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
        TradeRule ruleMACD;
        TradeRule ruleADX;

        TradeRule[] allRules = { };

        // ------------------------------

        private double openPosition = 0;
        internal long LastBuyTime = 0;
        internal int TradeTimeBlock = 30;
        internal long LastSellTime = 0;

        internal const double minTradeAmount = 0.0001;

        private Data.Predictors.PriceExtremes predictorExtremes;

        // ------------------------------

        public void Setup () {

            GUI.GUIManager.AddStrategyScreenPair(this.pair);

            // ----------------------------------

            ruleDelayAllTrades = new RuleDelayAllTrade();
            ruleDelayBuy = new RuleDelayBuy();

            ruleMinBase = new RuleMinimumBaseAmount();
            ruleMinBasePost = new RuleMinimumBaseAmountPost();
            ruleMinQuote = new RuleMinimumQuoteAmount();
            ruleMinQuotePost = new RuleMinimumQuoteAmountPost();

            ruleMinSellprice = new RuleMinimumSellPrice();
            ruleSellBand = new RuleSellBand();
            ruleStopLoss = new RuleStopLoss();

            rulePriceDelta = new RulePriceDelta();
            ruleMeanRev = new RuleMeanRev();
            ruleMACD = new RuleMACD();
            ruleADX = new RuleADX();

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss,  // sell rules
                rulePriceDelta, ruleMeanRev, ruleADX, ruleMACD }; // buy rules

            // ----------------------------------

            LastBuyTime = 0;
            TradeTimeBlock = 30;
            LastSellTime = 0;

            LastBuyTime = 0;
            openPosition = 0;

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);

        }

        public void Recalculate (long timestamp, double price, double[] values) {
            // adx, boll, macd, meanRev, priceDelta

            TickerChangedEventArgs tempTicker = new TickerChangedEventArgs(pair, new PoloniexAPI.MarketTools.MarketData(price));
            tempTicker.Timestamp = timestamp;

            predictorExtremes.Update(new TickerChangedEventArgs[] { tempTicker });

            Utility.TradeTracker.UpdateOpenPosition(pair, price);

            // ------------------------------

            double lastPrice = price;
            double buyPrice = price;
            double sellPrice = price;

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currBaseAmount = Manager.GetWalletState(pair.BaseCurrency);

            double currTradableBaseAmount = currBaseAmount * 0.3; // VolatilityScore;

            double postBaseAmount = currQuoteAmount * buyPrice;
            double postQuoteAmount = currTradableBaseAmount / sellPrice;

            double adx = values[0];
            double boll = values[1];
            double macd = values[2];
            double meanRev = values[3];
            double priceDelta = values[4];
            double minPrice = 0;
            double maxPrice = 0;

            // --------------------------

            Data.ResultSet.Variable tempVar;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;

            // --------------------------

            // -------------------------------------------
            // Compile all the rule variables into a dictionary
            // -------------------------------------------

            Dictionary<string, double> ruleVariables = new Dictionary<string, double>();

            ruleVariables.Add("lastBuyTimestamp", LastBuyTime);
            ruleVariables.Add("lastSellTimestamp", LastSellTime);
            ruleVariables.Add("lastTickerTimestamp", timestamp);

            ruleVariables.Add("price", lastPrice);
            ruleVariables.Add("buyPrice", buyPrice);
            ruleVariables.Add("sellPrice", sellPrice);

            ruleVariables.Add("openPrice", openPosition);

            ruleVariables.Add("quoteAmount", currQuoteAmount);
            ruleVariables.Add("baseAmount", currBaseAmount);

            ruleVariables.Add("baseAmountTradable", currTradableBaseAmount);

            ruleVariables.Add("postQuoteAmount", postQuoteAmount);
            ruleVariables.Add("postBaseAmount", postBaseAmount);

            ruleVariables.Add("adx", adx);
            ruleVariables.Add("bandSizeDelta", boll);
            ruleVariables.Add("macd", macd);
            ruleVariables.Add("meanRev", meanRev);
            ruleVariables.Add("priceDelta", priceDelta);
            
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

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell && ruleDelayBuy.Result != RuleResult.BlockBuy) {
                    // enough time has passed since the last trades were made

                    if (ruleMinQuote.Result == RuleResult.BlockSell) {
                        // if it's blocking sell that means we don't own quote, so go ahead with buying

                        if (ruleADX.currentResult == RuleResult.Buy && ruleMeanRev.currentResult == RuleResult.Buy) {
                            // price has stopped falling and is below average

                            Buy(sellPrice, postQuoteAmount, timestamp);
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Sell Logic Tree
            if (ruleMinQuote.Result != RuleResult.BlockSell && ruleMinBasePost.Result != RuleResult.BlockSell) {
                // we have enough of quote and will have enough of base (after trade) to satisfy minimum trade amount (0.0001)

                if (ruleDelayAllTrades.Result != RuleResult.BlockBuySell) {
                    // enough time has passed since the last trades were made

                    if (ruleMinSellprice.Result != RuleResult.BlockSell) {
                        // current price is profitable

                        if (ruleSellBand.Result == RuleResult.Sell) {
                            // price is below the sell band

                            Sell(buyPrice, currQuoteAmount, timestamp);
                            return;
                        }
                    }

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // price has dropped below stop-loss

                        Sell(buyPrice, currQuoteAmount, timestamp);
                        return;
                    }
                }
            }
            #endregion

        }


        private void Buy (double sellPrice, double quoteAmount, long timestamp) {

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
                    LastBuyTime = timestamp;

                    Utility.TradeTracker.ReportBuy(pair, quoteAmount, sellPrice, LastBuyTime);

                    openPosition = sellPrice;
                    predictorExtremes.CurrentMaximum = sellPrice;
                    predictorExtremes.CurrentMinimum = sellPrice;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making buy: " + e.Message + "\n" + e.StackTrace);
            }
        }
        private void Sell (double buyPrice, double quoteAmount, long timestamp) {

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
                    LastSellTime = timestamp;

                    Utility.TradeTracker.ReportSell(pair, quoteAmount, buyPrice, LastSellTime);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message + "\n" + e.StackTrace);
            }
        }

    }
}

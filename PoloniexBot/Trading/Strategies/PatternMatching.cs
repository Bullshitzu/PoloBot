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
            Windows.Controls.StrategyScreen.drawVariables = new string[] { "buySignal", "meanRev" };
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

        TradeRule rulePatternMatch;
        TradeRule ruleMeanRev;

        TradeRule[] allRules = { };

        RuleGlobalDrop ruleGlobalTrend;

        // ------------------------------

        private double openPosition = 0;

        private Data.Predictors.PriceExtremes predictorExtremes;
        private Data.Predictors.PatternMatch predictorPatternMatch;

        private Data.Predictors.MeanReversion predictorMeanRev;
        private Data.Predictors.BollingerBands predictorBollinger;
        private Data.Predictors.MACD predictorMACD;
        private Data.Predictors.ADX predictorADX;

        public override void Setup (bool simulate = false) {

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

            rulePatternMatch = new RulePatternMatch();
            ruleMeanRev = new RuleMeanRev();

            ruleGlobalTrend = new RuleGlobalDrop("meanRev");

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss, // sell rules
                rulePatternMatch, ruleMeanRev }; // buy rules

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorPatternMatch = new Data.Predictors.PatternMatch(pair);

            predictorMeanRev = new Data.Predictors.MeanReversion(pair);
            predictorBollinger = new Data.Predictors.BollingerBands(pair);
            predictorMACD = new Data.Predictors.MACD(pair);
            predictorADX = new Data.Predictors.ADX(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);

            predictorMeanRev.Recalculate(tickers);
            predictorBollinger.Recalculate(tickers);
            predictorMACD.Recalculate(tickers);
            predictorADX.Recalculate(tickers);

            // Create and check the new pattern

            Data.PatternMatching.Pattern p = Data.PatternMatching.Manager.GeneratePattern(tickers);
            List<double> patternData = new List<double>(p.movement);
            patternData.AddRange(GetPredictorValues());
            p.movement = patternData.ToArray();

            predictorPatternMatch.Recalculate(p, tickers.Last().Timestamp);
        }

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorPatternMatch = null;

            predictorMeanRev = null;
            predictorBollinger = null;
            predictorMACD = null;
            predictorADX = null;

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
            predictorBollinger.Recalculate(tickers);
            predictorMACD.Recalculate(tickers);
            predictorADX.Recalculate(tickers);

            // Create and check the new pattern

            Data.PatternMatching.Pattern p = Data.PatternMatching.Manager.GeneratePattern(tickers);
            List<double> patternData = new List<double>(p.movement);
            patternData.AddRange(GetPredictorValues());
            p.movement = patternData.ToArray();

            predictorPatternMatch.Recalculate(p, tickers.Last().Timestamp);

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

            double buySignal = double.MaxValue;
            double sellSignal = double.MaxValue;

            double meanRev = 0;
            double macd = 0;

            Data.ResultSet.Variable tempVar;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;
            if (predictorPatternMatch.GetLastResult().variables.TryGetValue("buySignal", out tempVar)) buySignal = tempVar.value;
            if (predictorPatternMatch.GetLastResult().variables.TryGetValue("sellSignal", out tempVar)) sellSignal = tempVar.value;

            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;
            if (predictorMACD.GetLastResult().variables.TryGetValue("macd", out tempVar)) macd = tempVar.value;

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

            ruleVariables.Add("minPrice", minPrice);
            ruleVariables.Add("maxPrice", maxPrice);

            ruleVariables.Add("minGUI", lastPrice / minPrice);
            ruleVariables.Add("maxGUI", maxPrice / lastPrice);

            ruleVariables.Add("buySignal", buySignal);
            ruleVariables.Add("sellSignal", sellSignal);

            ruleVariables.Add("meanRev", meanRev);

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

                        if (rulePatternMatch.Result == RuleResult.Buy && ruleMeanRev.Result == RuleResult.Buy) {
                            // price pattern indicates we should buy

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
                            // price has dropped below the sell band

                            Sell(buyPrice, currQuoteAmount);
                            return;
                        }
                    }

                    if (ruleStopLoss.Result == RuleResult.Sell) {
                        // price has dropped below stopLoss level

                        Sell(buyPrice, currQuoteAmount);
                        return;
                    }
                }
            }
            #endregion
        }

        public double[] GetPredictorValues () {

            double meanRev = 0;
            double bollinger = 0;
            double macd = 0;
            double adx = 0;

            Data.ResultSet.Variable tempVar;
            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;
            if (predictorBollinger.GetLastResult().variables.TryGetValue("bandSizeDelta", out tempVar)) bollinger = tempVar.value;
            if (predictorMACD.GetLastResult().variables.TryGetValue("macd", out tempVar)) macd = tempVar.value;
            if (predictorADX.GetLastResult().variables.TryGetValue("adx", out tempVar)) adx = tempVar.value;

            bollinger /= 100;
            adx /= 30;

            return new double[] { meanRev, bollinger, macd, adx };
        }

        private void Buy (double sellPrice, double quoteAmount) {

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

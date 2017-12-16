using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class MeanRevMACD : Strategy {

        public MeanRevMACD (CurrencyPair pair) : base(pair) {

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

        TradeRule ruleMacd;
        TradeRule ruleMeanRev;

        TradeRule[] allRules = { };

        // ------------------------------

        private double openPosition = 0;

        double optTimeframe = Data.Predictors.MeanReversion.drawTimeframe;
        double optTrigger = RuleMeanRev.BuyTrigger;

        private Data.Predictors.PriceExtremes predictorExtremes;
        private Data.Predictors.MeanReversion predictorMeanRev;
        private Data.Predictors.MACD predictorMacd;

        public override void Setup (bool simulated = false) {

            GUI.GUIManager.AddStrategyScreenPair(this.pair);

            // ----------------------------------

            try {
                Data.VariableAnalysis.OptimizedPairData opd = Data.VariableAnalysis.GetPairData(pair);
                optTimeframe = opd.MeanRevTimeframe;
                optTrigger = opd.MeanRevTrigger;
            }
            catch (Exception e) {
                CLI.Manager.PrintWarning("No optimized pair data for " + pair + "!");
            }

            optTrigger *= 1.5;

            // ----------------------------------

            SetupRules(optTrigger);

            // ----------------------------------

            // Check file if this has been bought already
            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);
            predictorMeanRev = new Data.Predictors.MeanReversion(pair, (int)optTimeframe);
            predictorMacd = new Data.Predictors.MACD(pair, (int)(optTimeframe / 3));

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorExtremes.Update(tickers);

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorMeanRev.Recalculate(tickerList.ToArray());
                predictorMacd.Recalculate(tickerList.ToArray());

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

            ruleMacd = new RuleMACD(0);
            ruleMeanRev = new RuleMeanRev(optTrigger);

            // order doesn't matter
            allRules = new TradeRule[] { 
                ruleDelayAllTrades, ruleDelayBuy, // time delay
                ruleForce, // manual utility
                ruleMinBase, ruleMinBasePost, // minimum base amount
                ruleMinQuote, ruleMinQuotePost, // minimum quote amount
                ruleMinSellprice, ruleSellBand, ruleStopLoss,  // sell rules
                ruleMacd, ruleMeanRev }; // buy rules
        }

        private double GetOPTMultiplier (double ch24) {
            double m = (ch24 / 20) + 2.5;
            if (m < 1.5) m = 1.5;
            if (m > 3) m = 3;
            return m;
        }

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;
            predictorMacd = null;
            predictorMeanRev = null;

            Setup(true);
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;
            double change24 = lastTicker.ChangeLast;

            double mult = GetOPTMultiplier(change24);
            ruleMeanRev.SetTrigger(mult);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorExtremes.Update(tickers);
            predictorMeanRev.Recalculate(tickers);
            predictorMacd.Recalculate(tickers);

            Utility.TradeTracker.UpdateOpenPosition(pair, buyPrice);

            GUI.GUIManager.SetPairSummary(this.pair, tickers, lastTicker.MarketData.Volume24HourBase);
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
            double macd = 0;

            Data.ResultSet.Variable tempVar;
            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("min", out tempVar)) minPrice = tempVar.value;
            if (predictorExtremes.GetLastResult().variables.TryGetValue("max", out tempVar)) maxPrice = tempVar.value;
            if (predictorMacd.GetLastResult().variables.TryGetValue("macd", out tempVar)) macd = tempVar.value;

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

            ruleVariables.Add("macd", macd);

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

                        if (ruleMacd.currentResult == RuleResult.Buy && ruleMeanRev.currentResult == RuleResult.Buy) {
                            // MACD has stopped falling and price is below average

                            debugMacd = macd;
                            debugMeanRev = meanRev;

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
                    /*
                    if (ruleMacd.currentResult == RuleResult.Sell) {
                        // price is currently dropping

                        Sell(buyPrice, currQuoteAmount);

                        SaveTradeData(false, LastSellTime - LastBuyTime);

                        return;
                    }
                    */
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

        // ---------------------------------------------

        double debugMacd = 0;
        double debugMeanRev = 0;

        private void SaveTradeData (bool profit, long timespan) {

            List<string> lines = new List<string>();

            int hours = (int)(timespan / 3600);
            int minutes = (int)((timespan % 3600) / 60);
            int seconds = (int)(timespan % 60);

            lines.Add("");
            lines.Add(pair.ToString());
            lines.Add(hours + ":" + minutes + ":" + seconds);
            lines.Add("Macd: " + debugMacd.ToString("F8"));
            lines.Add("MeanRev: " + debugMeanRev.ToString("F8"));
            lines.Add("");

            string filename = "Logs/Trades" + (profit ? "Good" : "Bad") + ".data";

            Utility.FileManager.SaveFileConcat(filename, lines.ToArray());

        }
    }
}

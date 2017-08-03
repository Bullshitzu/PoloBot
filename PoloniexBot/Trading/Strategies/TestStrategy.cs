using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class TestStrategy : Strategy {

        public TestStrategy (CurrencyPair pair) : base(pair) { }

        private double openPosition = 0;

        private Data.Predictors.MeanReversion predictorMeanRev;
        private Data.Predictors.BollingerBands predictorBollinger;
        private Data.Predictors.MACD predictorMACD;
        private Data.Predictors.ADX predictorADX;
        
        public override void Setup (bool simulate = false) {

            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            openPosition = openPos;

            predictorMeanRev = new Data.Predictors.MeanReversion(pair);
            predictorBollinger = new Data.Predictors.BollingerBands(pair);
            predictorMACD = new Data.Predictors.MACD(pair);
            predictorADX = new Data.Predictors.ADX(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorMeanRev.Recalculate(tickers);
            predictorBollinger.Recalculate(tickers);
            predictorMACD.Recalculate(tickers);
            predictorADX.Recalculate(tickers);

        }

        public override void Reset () {
            base.Reset();

            predictorMeanRev = null;
            predictorBollinger = null;
            predictorMACD = null;
            predictorADX = null;

            openPosition = 0;

            Setup(true);
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorMeanRev.Recalculate(tickers);
            predictorBollinger.Recalculate(tickers);
            predictorMACD.Recalculate(tickers);
            predictorADX.Recalculate(tickers);

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

            if (ruleForce.Result == RuleResult.Buy) {
                Buy(sellPrice, postQuoteAmount);
                return;
            }

            if (ruleForce.Result == RuleResult.Sell) {
                Sell(buyPrice, currQuoteAmount);
                return;
            }

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

                    ruleForce.currentResult = RuleResult.None;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error making sale: " + e.Message);
            }
        }
    }
}

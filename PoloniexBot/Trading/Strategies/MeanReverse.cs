using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    class MeanReverse : Strategy {

        public MeanReverse (CurrencyPair pair) : base(pair) { }

        Data.Predictors.ADX predictorADX;
        Data.Predictors.MeanReversion predictorMeanRev;

        private double BuyTrigger = 1.5;
        private double ADXBlock = 35;

        private double minimumSellPrice = 0;
        private double minimumSellPriceFactor = 1.015;

        private double maximumPrice = 0;
        private double maximumPriceFactor = 0.7;

        public override void Setup () {

            double openPos = Utility.TradeTracker.GetOpenPosition(pair);
            LastBuyTime = Utility.TradeTracker.GetOpenPositionBuyTime(pair);
            minimumSellPrice = openPos * minimumSellPriceFactor;

            predictorMeanRev = new Data.Predictors.MeanReversion(pair);
            predictorADX = new Data.Predictors.ADX(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            // now recalculate history
            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorADX.Recalculate(tickerList.ToArray());
                predictorMeanRev.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("MeanReverse");
            }
        }
        public override void UpdatePredictors () {
            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            if (tickers.Length > 0) {
                predictorADX.Recalculate(tickers);
                predictorMeanRev.Recalculate(tickers);

                double lastPrice = tickers[tickers.Length - 1].MarketData.PriceLast;
                if (lastPrice > maximumPrice) maximumPrice = lastPrice;

                Utility.TradeTracker.UpdateOpenPosition(pair, tickers[tickers.Length - 1].MarketData.OrderTopBuy);
            }
        }
        public override void EvaluateTrade () {

            Data.ResultSet RSAdx = predictorADX.GetLastResult();
            Data.ResultSet RSMeanRev = predictorMeanRev.GetLastResult();

            double adx = 0;
            double meanRev = 0;

            Data.ResultSet.Variable tempVar;
            if (RSAdx.variables.TryGetValue("adx", out tempVar)) adx = tempVar.value;
            if (RSMeanRev.variables.TryGetValue("score", out tempVar)) meanRev = tempVar.value;

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            if (lastTicker.Timestamp - LastBuyTime < TradeTimeBlock) return;
            // to create a minimum 30 second delay between individual trades
            // prevents multiple buy orders

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currQuoteTotal = Manager.GetWalletState(pair.QuoteCurrency) + Manager.GetWalletStateOrders(pair.QuoteCurrency);

            double score = meanRev;
            if (adx > ADXBlock) score = 0;

            // ---------------------------------------------------

            double sellPriceTrigger = ((maximumPrice - minimumSellPrice) * maximumPriceFactor) + minimumSellPrice;

            if (currQuoteAmount >= minTradeAmount) {
                if (buyPrice >= minimumSellPrice && buyPrice <= sellPriceTrigger) {
                    double baseAmount = currQuoteAmount * buyPrice;
                    if (baseAmount >= minTradeAmount) {
                        // -----------------------------
                        Console.WriteLine("Attempting Sell - " + pair);
                        Console.WriteLine("Price: " + buyPrice.ToString("F8") + ", Amount: " + currQuoteAmount.ToString("F8"));
                        // -----------------------------
                        Task<ulong> postOrderTask = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Sell, buyPrice, currQuoteAmount);
                        ulong id = postOrderTask.Result;

                        if (id == 0) {
                            Console.WriteLine("Error making sale");
                        }
                        else {
                            Utility.TradeTracker.ReportSell(pair, currQuoteAmount, buyPrice);

                            LastBuyTime = lastTicker.Timestamp;
                            LastSellTime = lastTicker.Timestamp;
                            minimumSellPrice = 0;
                        }
                    }
                }
            }
            else if (score > BuyTrigger) {
                if (currQuoteTotal < minTradeAmount && currQuoteAmount < minTradeAmount) {
                    double baseAmount = Manager.GetWalletState(pair.BaseCurrency) * VolatilityScore; // dont want to use it all on one pair
                    if (baseAmount >= minTradeAmount) {
                        double quoteAmount2 = baseAmount / sellPrice;
                        if (quoteAmount2 >= minTradeAmount) { // buy quote currency
                            // -----------------------------
                            Console.WriteLine("Attempting Buy - " + pair);
                            Console.WriteLine("Price: " + sellPrice.ToString("F8") + ", Amount: " + currQuoteAmount.ToString("F8"));
                            // -----------------------------
                            Task<ulong> postOrderTask = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, sellPrice, quoteAmount2);
                            ulong id = postOrderTask.Result;

                            if (id == 0) {
                                Console.WriteLine("Error making buy");
                            }
                            else {
                                Utility.TradeTracker.ReportBuy(pair, quoteAmount2, sellPrice);

                                LastBuyTime = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now) - 20;
                                minimumSellPrice = sellPrice * minimumSellPriceFactor;
                                maximumPrice = sellPrice;
                            }
                        }
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    class LowAlts : Strategy {

        public LowAlts (CurrencyPair pair) : base(pair) { }

        // buy volatile alt coin
        // if price drops by more then 15%, sell it
        // do the maxiumum band thing with sales
        // after N hours, sell it anyway and refresh tradepairs

        private double minimumSellPrice = 0;
        private double minimumSellPriceFactor = 1.0075; // +0.75% minimum profit

        private double maximumPrice = 0;

        private double macdBuyTrigger = 0.4;

        private double buyCooldown = 1800;

        private Data.Predictors.MACD predictorMacd;

        public override void Setup () {

            // Check file if this has been bought already
            double openPos = 0;
            Utility.TradeTracker.GetOpenPosition(pair, ref openPos);
            minimumSellPrice = openPos * minimumSellPriceFactor;

            maximumPrice = openPos;

            predictorMacd = new Data.Predictors.MACD(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);
                
                predictorMacd.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive();
            }
        }
        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorMacd.Recalculate(tickers);

            if (buyPrice > maximumPrice) maximumPrice = buyPrice;
            Utility.TradeTracker.UpdateOpenPosition(pair, buyPrice);
        }
        public override void EvaluateTrade () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            if (lastTicker.Timestamp - LastTradeTime < TradeTimeBlock) return;
            // to create a minimum 30 second delay between individual trades
            // prevents multiple buy orders

            if (lastTicker.Timestamp - lastSellTime < buyCooldown) return;
            // to create a minimum 30 minute delay from sale to new buy

            Data.ResultSet.Variable tempVar;
            double macd = 0;

            Data.ResultSet RSMacd = predictorMacd.GetLastResult();

            if (RSMacd.variables.TryGetValue("macdHistogramAdjusted", out tempVar)) macd = tempVar.value;

            double currQuoteAmount = Manager.GetWalletState(pair.QuoteCurrency);
            double currQuoteTotal = Manager.GetWalletState(pair.QuoteCurrency) + Manager.GetWalletStateOrders(pair.QuoteCurrency);

            // -----------------------------------------------

            // SELL
            if (currQuoteAmount >= minTradeAmount) {
                double sellBandSize = ((maximumPrice - minimumSellPrice) / minimumSellPrice) * 100;
                double priceBandFactor = ((buyPrice - minimumSellPrice) / minimumSellPrice) * 100;
                if (sellBandSize < 0 || priceBandFactor < 0) return;

                if (sellBandSize > 17) sellBandSize = 17; // to lock a minimum of 2% band size on high values

                double sellPriceTrigger = (0.04074 * Math.Pow(sellBandSize, 2)) + (0.263 * sellBandSize) - 1.454;
                // 0.04074x^2 + 0.2630x − 1.454
                // that's taking into account the minimumSellPriceFactor
                // It's actually -1, 1, 6
                // -5		-1.75
                // 4		0.25
                // 10		5.25

                if (buyPrice >= minimumSellPrice && priceBandFactor <= sellPriceTrigger) {
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

                            lastTradeTime = lastTicker.Timestamp;
                            lastSellTime = lastTicker.Timestamp;
                        }
                    }
                }
            }
            // BUY
            else if (minimumSellPrice == 0 && macd > macdBuyTrigger) {
                if (currQuoteTotal < minTradeAmount && currQuoteAmount < minTradeAmount) {
                    double baseAmount = Manager.GetWalletState(pair.BaseCurrency) * VolatilityScore; // dont want to use it all on one pair
                    if (baseAmount >= minTradeAmount) {
                        double quoteAmount2 = baseAmount / sellPrice;
                        if (quoteAmount2 >= minTradeAmount) { // buy quote currency
                            // -----------------------------
                            Console.WriteLine("Attempting Buy - " + pair);
                            Console.WriteLine("Price: " + sellPrice.ToString("F8") + ", Amount: " + quoteAmount2.ToString("F8"));
                            // -----------------------------
                            Task<ulong> postOrderTask = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(pair, OrderType.Buy, sellPrice, quoteAmount2);
                            ulong id = postOrderTask.Result;

                            if (id == 0) {
                                Console.WriteLine("Error making buy");
                            }
                            else {
                                Utility.TradeTracker.ReportBuy(pair, quoteAmount2, sellPrice);

                                lastTradeTime = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now) - 20;
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

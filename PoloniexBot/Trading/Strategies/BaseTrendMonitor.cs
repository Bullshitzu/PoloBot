using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using PoloniexBot.Trading.Rules;

namespace PoloniexBot.Trading.Strategies {
    class BaseTrendMonitor : Strategy {

        public BaseTrendMonitor (CurrencyPair pair) : base(pair) {
            PullTickerHistoryHours = 27;
        }

        // ------------------------------

        public override void Reset () {
            base.Reset();

            openPosition = 0;

            predictorExtremes = null;

            Setup(true);
        }

        // ------------------------------

        public static double LastUSDTBTCPrice = 0;
        
        // ------------------------------

        private ulong orderID = 0;
        private double orderPrice = 0;

        private double openPosition = 0;

        private Data.Predictors.PriceExtremes predictorExtremes;

        // ------------------------------

        public override void Setup (bool simulate = false) {

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            if (tickers.Length > 0) LastUSDTBTCPrice = tickers.Last().MarketData.PriceLast;
            Manager.UpdateWalletValue("USDT", LastUSDTBTCPrice);

            predictorExtremes = new Data.Predictors.PriceExtremes(pair);

            predictorExtremes.Update(tickers);

            GUI.GUIManager.UpdateMainSummary(tickers);
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs lastTicker = Data.Store.GetLastTicker(pair);
            double lastPrice = lastTicker.MarketData.PriceLast;
            double buyPrice = lastTicker.MarketData.OrderTopBuy;
            double sellPrice = lastTicker.MarketData.OrderTopSell;

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            if(tickers.Length > 0) LastUSDTBTCPrice = tickers.Last().MarketData.PriceLast;
            Manager.UpdateWalletValue("USDT", LastUSDTBTCPrice);

            predictorExtremes.Update(tickers);
            
            GUI.GUIManager.UpdateMainSummary(tickers);
        }

        public override void EvaluateTrade () {
            // do nothing
        }
    }
}

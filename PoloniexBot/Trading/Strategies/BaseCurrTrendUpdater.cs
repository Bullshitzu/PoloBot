using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    class BaseCurrTrendUpdater : Strategy {

        public BaseCurrTrendUpdater (CurrencyPair pair) : base(pair) {
            PullTickerHistoryHours = 27;
        }

        public override void Reset () {
            base.Reset();
            Setup(true);
        }

        // ------------------------------

        public static double LastUSDTBTCPrice = 0;
        public static double LastUSDTBTCMeanRev = 0;

        // ------------------------------

        private Data.Predictors.MeanReversion predictorMeanRev;

        // ------------------------------

        public override void Setup (bool simulate = false) {

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            if (tickers.Length > 0) LastUSDTBTCPrice = tickers.Last().MarketData.PriceLast;

            predictorMeanRev = new Data.Predictors.MeanReversion(pair);

            List<TickerChangedEventArgs> tickerList = new List<TickerChangedEventArgs>();

            for (int i = 0; i < tickers.Length; i++) {
                tickerList.Add(tickers[i]);

                predictorMeanRev.Recalculate(tickerList.ToArray());

                if (i % 100 == 0) Utility.ThreadManager.ReportAlive("BaseCurrTrendUpdater");
            }

            GUI.GUIManager.UpdateMainSummary(tickers);
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            if(tickers.Length > 0) LastUSDTBTCPrice = tickers.Last().MarketData.PriceLast;

            predictorMeanRev.Recalculate(tickers);

            GUI.GUIManager.UpdateMainSummary(tickers);

        }

        public override void EvaluateTrade () {

            Data.ResultSet.Variable tempVar;
            if (predictorMeanRev.GetLastResult().variables.TryGetValue("score", out tempVar)) LastUSDTBTCMeanRev = tempVar.value;

        }
    }
}

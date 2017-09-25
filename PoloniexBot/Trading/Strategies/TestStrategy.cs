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

        public static double USDT_BTC_Trend = 0;

        private Data.Predictors.MACD predictorMACD;

        public override void Setup (bool simulate = false) {

            predictorMACD = new Data.Predictors.MACD(pair);

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Couldn't build predictor history for " + pair + " - no tickers available");

            predictorMACD.Recalculate(tickers);

            Data.ResultSet.Variable tempVar;
            if (predictorMACD.GetLastResult().variables.TryGetValue("macd", out tempVar)) USDT_BTC_Trend = tempVar.value;
        }

        public override void Reset () {
            base.Reset();

            predictorMACD = null;

            Setup(true);
        }

        public override void UpdatePredictors () {

            TickerChangedEventArgs[] tickers = Data.Store.GetTickerData(pair);
            if (tickers == null) throw new Exception("Data store returned NULL tickers for pair " + pair);

            predictorMACD.Recalculate(tickers);

            Data.ResultSet.Variable tempVar;
            if (predictorMACD.GetLastResult().variables.TryGetValue("macd", out tempVar)) USDT_BTC_Trend = tempVar.value;
        }

        public override void EvaluateTrade () {
            // do nothing
        }
    }
}

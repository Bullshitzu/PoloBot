using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    abstract class Strategy {

        internal CurrencyPair pair;

        internal long LastTradeTime = 0;
        internal int TradeTimeBlock = 30;

        internal const double minTradeAmount = 0.0001;

        internal long lastTradeTime = 0;
        internal long lastSellTime = 0;

        internal double VolatilityScore = 0;

        public Strategy (CurrencyPair pair) {
            this.pair = pair;
        }

        public void SetVolatility (double value) {
            this.VolatilityScore = value;
        }

        public abstract void Setup (); // Called on TPManager initialization, after data pull
        public abstract void UpdatePredictors (); // Called on ticker update
        public abstract void EvaluateTrade (); // Called after Update, handle buy/sell here

    }
}

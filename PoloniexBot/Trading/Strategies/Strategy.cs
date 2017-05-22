using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    abstract class Strategy {

        internal CurrencyPair pair;

        internal long LastBuyTime = 0;
        internal int TradeTimeBlock = 30;
        internal long LastSellTime = 0;

        internal const double minTradeAmount = 0.0001;

        internal double VolatilityScore = 0;

        internal Rules.TradeRule ruleForce;

        public Strategy (CurrencyPair pair) {
            this.pair = pair;
        }

        public void SetVolatility (double value) {
            this.VolatilityScore = value;
        }

        public abstract void Setup (); // Called on TPManager initialization, after data pull
        public abstract void UpdatePredictors (); // Called on ticker update
        public abstract void EvaluateTrade (); // Called after Update, handle buy/sell here

        public void ForceBuy () {
            ruleForce.currentResult = Rules.RuleResult.Buy;
            EvaluateTrade();
        }
        public void ForceSell () {
            ruleForce.currentResult = Rules.RuleResult.Sell;
            EvaluateTrade();
        }

    }
}
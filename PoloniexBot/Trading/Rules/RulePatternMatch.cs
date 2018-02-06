using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePatternMatch : TradeRule {

        public const double BuyTrigger = 2;
        public const double SellTrigger = -2;

        private double localBuyTrigger = BuyTrigger;
        private double localSellTrigger = SellTrigger;

        public RulePatternMatch (double trigger) {
            localBuyTrigger = trigger;
            localSellTrigger = -trigger;
        }

        public override void Recalculate (Dictionary<string, double> values) {

            double signal = 0;

            if (!values.TryGetValue("patternMatchResult", out signal)) throw new VariableNotIncludedException("patternMatchResult");

            if (signal > localBuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            if (signal < localSellTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

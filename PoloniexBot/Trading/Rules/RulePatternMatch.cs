using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePatternMatch : TradeRule {

        public static double BuyTrigger = 2;
        public static double SellTrigger = -2;

        public override void Recalculate (Dictionary<string, double> values) {

            double signal = 0;

            if (!values.TryGetValue("patternMatchResult", out signal)) throw new VariableNotIncludedException("patternMatchResult");

            if (signal > BuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            if (signal < SellTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

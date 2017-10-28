using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePatternMatch : TradeRule {

        public static double BuyTrigger = 0.3;

        public override void Recalculate (Dictionary<string, double> values) {

            double buySignal = 0;

            if (!values.TryGetValue("buySignal", out buySignal)) throw new VariableNotIncludedException("buySignal");

            if (buySignal > BuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

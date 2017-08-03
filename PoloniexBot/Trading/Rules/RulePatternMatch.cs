using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePatternMatch : TradeRule {

        public static double BuyTrigger = 0.5;
        public static double SellTrigger = 2;

        public override void Recalculate (Dictionary<string, double> values) {

            double buySignal = double.MaxValue;
            double sellSignal = double.MaxValue;

            if (!values.TryGetValue("buySignal", out buySignal)) throw new VariableNotIncludedException("buySignal");
            if (!values.TryGetValue("sellSignal", out sellSignal)) throw new VariableNotIncludedException("sellSignal");

            if (buySignal < BuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            if (sellSignal < SellTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

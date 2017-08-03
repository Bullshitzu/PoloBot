using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePriceDelta : TradeRule {

        public static double Trigger1 = 0.5;
        public static double Trigger2 = 1;
        public static double Trigger3 = 2;

        public override void Recalculate (Dictionary<string, double> values) {

            double priceDelta1 = 0;
            double priceDelta2 = 0;
            double priceDelta3 = 0;

            if (!values.TryGetValue("priceDelta1", out priceDelta1)) throw new VariableNotIncludedException("priceDelta1");
            if (!values.TryGetValue("priceDelta2", out priceDelta2)) throw new VariableNotIncludedException("priceDelta2");
            if (!values.TryGetValue("priceDelta3", out priceDelta3)) throw new VariableNotIncludedException("priceDelta3");

            if (priceDelta1 > Trigger1 && priceDelta2 > Trigger2 && priceDelta3 > Trigger3) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

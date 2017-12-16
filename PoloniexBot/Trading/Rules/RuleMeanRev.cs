using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMeanRev : TradeRule {

        public static double BuyTrigger = -4; // price N% below 3hour mean
        private double localTrigger = -4;

        public RuleMeanRev () {
            localTrigger = BuyTrigger;
        }

        public RuleMeanRev (double trigger) {
            localTrigger = trigger * 1;
        }

        public override void SetTrigger (params double[] values) {
            localTrigger = values[0];
        }

        public override void Recalculate (Dictionary<string, double> values) {

            double meanRev = 0;

            if (!values.TryGetValue("meanRev", out meanRev)) throw new VariableNotIncludedException("meanRev");

            if (meanRev < localTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

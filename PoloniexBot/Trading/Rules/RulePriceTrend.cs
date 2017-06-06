using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePriceTrend : TradeRule {

        private const double delta0Trigger = 1.2;
        private const double delta1Trigger = 0.8;
        private const double delta2Trigger = 0.5;

        public override void Recalculate (Dictionary<string, double> values) {

            double delta0 = 0;
            double delta1 = 0;
            double delta2 = 0;

            if (!values.TryGetValue("delta0", out delta0)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("delta1", out delta1)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("delta2", out delta2)) throw new VariableNotIncludedException();

            if (delta0 > delta0Trigger &&
                delta1 > delta1Trigger &&
                delta2 > delta2Trigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

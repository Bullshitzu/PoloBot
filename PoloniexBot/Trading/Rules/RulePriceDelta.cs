using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePriceDelta : TradeRule {

        private const double Trigger0 = 0.5;
        private const double TriggerAvg = 0.5;

        public override void Recalculate (Dictionary<string, double> values) {

            double delta0 = 0;
            double deltaAvg = 0;

            if (!values.TryGetValue("deltaAvg", out deltaAvg)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("delta0", out delta0)) throw new VariableNotIncludedException();

            if (deltaAvg > TriggerAvg && delta0 > Trigger0) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

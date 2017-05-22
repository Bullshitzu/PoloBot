using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePriceDelta : TradeRule {

        private const double Trigger = 2.5;

        public override void Recalculate (Dictionary<string, double> values) {
            
            double delta = 0;

            if (!values.TryGetValue("deltaShort", out delta)) throw new VariableNotIncludedException();

            if (delta > Trigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

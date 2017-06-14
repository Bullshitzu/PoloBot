using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBollinger : TradeRule {

        public override void Recalculate (Dictionary<string, double> values) {

            double bandSizeDelta = 0;

            if (!values.TryGetValue("bollingerBandDelta", out bandSizeDelta)) throw new VariableNotIncludedException();

            if (bandSizeDelta < 0) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

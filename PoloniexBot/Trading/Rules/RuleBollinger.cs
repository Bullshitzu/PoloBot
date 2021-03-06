using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBollinger : TradeRule {

        public static double Trigger = -20;

        public override void Recalculate (Dictionary<string, double> values) {

            double bandSizeDelta = 0;

            if (!values.TryGetValue("bandSizeDelta", out bandSizeDelta)) throw new VariableNotIncludedException("bandSizeDelta");

            if (bandSizeDelta < Trigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

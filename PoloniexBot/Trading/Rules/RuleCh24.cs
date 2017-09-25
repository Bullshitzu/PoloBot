using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleCh24 : TradeRule {

        public double localTrigger = 10;

        public override void Recalculate (Dictionary<string, double> values) {

            double ch24;

            if (!values.TryGetValue("ch24", out ch24)) ch24 = double.MinValue;

            if (ch24 > localTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;

        }
    }
}

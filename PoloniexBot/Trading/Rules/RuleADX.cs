using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleADX : TradeRule {

        public RuleADX (double trigger) {
            Trigger = trigger;
        }
        
        private double Trigger;

        public override void Recalculate (Dictionary<string, double> values) {

            double adx = 0;

            if (!values.TryGetValue("adx", out adx)) throw new VariableNotIncludedException();

            if (adx > Trigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;

        }
    }
}

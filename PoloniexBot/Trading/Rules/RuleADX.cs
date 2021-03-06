using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleADX : TradeRule {

        public static double Trigger = 90;
        private double localTrigger = 90;

        public RuleADX () {
            localTrigger = Trigger;
        }

        public RuleADX (double trigger) {
            localTrigger = trigger;
        }

        public override void SetTrigger (params double[] values) {
            localTrigger = values[0];
        }

        public override void Recalculate (Dictionary<string, double> values) {

            double adx = 0;

            if (!values.TryGetValue("adx", out adx)) throw new VariableNotIncludedException();

            if (adx < localTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMACD : TradeRule {

        public static double MacdTrigger = 0.015;
        private double localTrigger = 0.015;

        public RuleMACD () {
            localTrigger = MacdTrigger;
        }
        public RuleMACD (double trigger) {
            localTrigger = trigger;
        }

        public override void SetTrigger (params double[] values) {
            localTrigger = values[0];
        }

        public override void Recalculate (Dictionary<string, double> values) {

            double macd = 0;

            if (!values.TryGetValue("macd", out macd)) throw new VariableNotIncludedException();

            if (macd > localTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            if (macd < -localTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

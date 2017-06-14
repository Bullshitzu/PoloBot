using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMACD : TradeRule {

        private const double MacdTrigger = 0.2;

        public override void Recalculate (Dictionary<string, double> values) {

            double macd = 0;

            if (!values.TryGetValue("macd", out macd)) throw new VariableNotIncludedException();

            if (macd > MacdTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

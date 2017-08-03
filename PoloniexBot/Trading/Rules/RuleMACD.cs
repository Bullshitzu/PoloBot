using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMACD : TradeRule {

        public static double MacdTrigger = -0.09;

        public override void Recalculate (Dictionary<string, double> values) {

            double macd = 0;

            if (!values.TryGetValue("macd", out macd)) throw new VariableNotIncludedException();

            if (macd > MacdTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            if (macd < MacdTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

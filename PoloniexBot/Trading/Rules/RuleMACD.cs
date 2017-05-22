using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMACD : TradeRule {

        private const double MacdBuyTriggerShort = 2;
        private const double MacdBuyTriggerLong = 2;

        public override void Recalculate (Dictionary<string, double> values) {

            double macdShort = 0;
            double macdLong = 0;

            if (!values.TryGetValue("macdShort", out macdShort)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("macdLong", out macdLong)) throw new VariableNotIncludedException();

            if (macdShort > MacdBuyTriggerShort && macdLong > MacdBuyTriggerLong) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

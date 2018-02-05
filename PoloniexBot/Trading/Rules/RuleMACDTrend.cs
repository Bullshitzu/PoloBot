using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMACDTrend : TradeRule {

        public static double BuyTrigger = 0;
        public static double SellTrigger = -3; // note: irrelevant, not used

        public override void Recalculate (Dictionary<string, double> values) {

            double macdTrend = 0;

            if (!values.TryGetValue("macdTrend", out macdTrend)) throw new VariableNotIncludedException();

            if (macdTrend > BuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            if (macdTrend < SellTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

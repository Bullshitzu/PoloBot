using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleDump : TradeRule {

        private const int DumpTime = 10800;

        public override void Recalculate (Dictionary<string, double> values) {

            double lastTickerTimestamp = 0;
            double lastBuyTimestamp = 0;

            if (!values.TryGetValue("lastTickerTimestamp", out lastTickerTimestamp)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("lastBuyTimestamp", out lastBuyTimestamp)) throw new VariableNotIncludedException();

            if (lastTickerTimestamp - lastBuyTimestamp > DumpTime) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

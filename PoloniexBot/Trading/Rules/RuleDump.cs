using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleDump : TradeRule {

        private const int DumpTime = 21600; // 6 hours
        private const double DumpDrop = 0.9; // 10%

        public override void Recalculate (Dictionary<string, double> values) {

            double lastTickerTimestamp = 0;
            double lastBuyTimestamp = 0;

            if (!values.TryGetValue("lastTickerTimestamp", out lastTickerTimestamp)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("lastBuyTimestamp", out lastBuyTimestamp)) throw new VariableNotIncludedException();

            if (lastTickerTimestamp - lastBuyTimestamp > DumpTime) {
                currentResult = RuleResult.Sell;
                return;
            }

            // --------------------------------------------

            double minPrice = 0;
            double openPrice = 0;

            if (!values.TryGetValue("minPrice", out minPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException();

            if (minPrice < openPrice * DumpDrop) {
                currentResult = RuleResult.Sell;
                return;
            }

            // --------------------------------------------

            currentResult = RuleResult.None;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleStopLoss : TradeRule {

        private const double StopLossTrigger = 0.90; // 10% drop = sell

        public override void Recalculate (Dictionary<string, double> values) {

            double openPrice = 0;
            double currBuyPrice = 0;

            if (!values.TryGetValue("buyPrice", out currBuyPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException();

            if (openPrice * StopLossTrigger > currBuyPrice) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

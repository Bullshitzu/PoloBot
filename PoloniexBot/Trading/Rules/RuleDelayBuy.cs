using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleDelayBuy : TradeRule {

        private const long BuyCooldown = 180; // 3 minutes block after selling

        public override void Recalculate (Dictionary<string, double> values) {
            
            double lastTickerTimestamp = 0;
            double lastSellTimestamp = 0;

            if (!values.TryGetValue("lastTickerTimestamp", out lastTickerTimestamp)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("lastSellTimestamp", out lastSellTimestamp)) throw new VariableNotIncludedException();

            if (lastTickerTimestamp - lastSellTimestamp < BuyCooldown) {
                currentResult = RuleResult.BlockBuy;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

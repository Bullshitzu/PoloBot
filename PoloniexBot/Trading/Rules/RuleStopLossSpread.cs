using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleStopLossSpread : TradeRule {

        public RuleStopLossSpread (double triggerStart = StopLossTrigger) {
            localTrigger = triggerStart;
        }

        private const double StopLossTrigger = 0.85; // 3% drop = sell
        private const int TimeTrigger = 54000; // 24 hours

        private double localTrigger;

        public override void Recalculate (Dictionary<string, double> values) {

            double openPrice = 0;
            double currSellPrice = 0;

            double currTimestamp = 0;
            double buyTimestamp = 0;

            if (!values.TryGetValue("sellPrice", out currSellPrice)) throw new VariableNotIncludedException("sellPrice");
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException("openPrice");

            if (!values.TryGetValue("lastTickerTimestamp", out currTimestamp)) throw new VariableNotIncludedException("lastTickerTimestamp");
            if (!values.TryGetValue("lastBuyTimestamp", out buyTimestamp)) throw new VariableNotIncludedException("lastBuyTimestamp");

            double timeTriggerOffset = ((currTimestamp - buyTimestamp) / TimeTrigger) * 0.1;

            if (openPrice * (localTrigger + timeTriggerOffset) > currSellPrice) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

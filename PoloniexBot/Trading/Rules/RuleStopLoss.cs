using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleStopLoss : TradeRule {

        public RuleStopLoss (double triggerStart = StopLossTrigger) {
            localTrigger = triggerStart;
            currTrigger = triggerStart;
        }

        private const double StopLossTrigger = 0.8;
        private const int TimeTrigger = 86400;

        // 3 hours = 10%

        private double localTrigger;
        public double currTrigger;

        public override void Recalculate (Dictionary<string, double> values) {

            double openPrice = 0;
            double currBuyPrice = 0;

            double currTimestamp = 0;
            double buyTimestamp = 0;

            if (!values.TryGetValue("buyPrice", out currBuyPrice)) throw new VariableNotIncludedException("buyPrice");
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException("openPrice");

            if (!values.TryGetValue("lastTickerTimestamp", out currTimestamp)) throw new VariableNotIncludedException("lastTickerTimestamp");
            if (!values.TryGetValue("lastBuyTimestamp", out buyTimestamp)) throw new VariableNotIncludedException("lastBuyTimestamp");

            double timeTriggerOffset = ((currTimestamp - buyTimestamp) / TimeTrigger) * 0.05;
            double val = openPrice * (localTrigger + timeTriggerOffset);

            currTrigger = (localTrigger + timeTriggerOffset);

            if (val > currBuyPrice) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

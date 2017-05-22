using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleDelayAllTrade : TradeRule {

        private const long TradeTimeBlock = 30; // 30 second block after selling or buying

        public override void Recalculate (Dictionary<string, double> values) {

            double LastBuyTimestamp = 0;
            double lastSellTimestamp = 0;
            double lastTickerTimestamp = 0;

            if (!values.TryGetValue("lastTickerTimestamp", out lastTickerTimestamp)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("lastSellTimestamp", out lastSellTimestamp)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("lastBuyTimestamp", out LastBuyTimestamp)) throw new VariableNotIncludedException();

            if (lastTickerTimestamp - lastSellTimestamp < TradeTimeBlock) {
                currentResult = RuleResult.BlockBuySell;
                return;
            }
            if (lastTickerTimestamp - LastBuyTimestamp < TradeTimeBlock) {
                currentResult = RuleResult.BlockBuySell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

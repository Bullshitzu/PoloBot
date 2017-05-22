using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumSellPrice : TradeRule {

        public const double ProfitFactor = 1.0075; // +0.75% minimum profit

        public override void Recalculate (Dictionary<string, double> values) {

            double currBuyPrice = 0; // current price
            double openPrice = 0; // price at which it was bought

            if (!values.TryGetValue("buyPrice", out currBuyPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException();

            if (currBuyPrice < openPrice * ProfitFactor) {
                currentResult = RuleResult.BlockSell;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumSellPriceGiver : TradeRule {
        public const double ProfitFactor = 1.015; // +15% minimum profit

        public override void Recalculate (Dictionary<string, double> values) {

            double currSellPrice = 0; // current price
            double openPrice = 0; // price at which it was bought

            if (!values.TryGetValue("sellPrice", out currSellPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException();

            if (currSellPrice < openPrice * ProfitFactor) {
                currentResult = RuleResult.BlockSell;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

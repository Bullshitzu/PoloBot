using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBuyBand : TradeRule {
        public override void Recalculate (Dictionary<string, double> values) {
            
            double currSellPrice = 0;
            double minimumPrice = 0;

            if (!values.TryGetValue("sellPrice", out currSellPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("minPrice", out minimumPrice)) throw new VariableNotIncludedException();

            double currPriceDeltaPercent = currSellPrice / minimumPrice;

            if (currPriceDeltaPercent > 1.015) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

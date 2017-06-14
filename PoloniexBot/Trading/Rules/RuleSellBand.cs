using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleSellBand : TradeRule {
        public override void Recalculate (Dictionary<string, double> values) {

            double currBuyPrice = 0;
            double openPrice = 0;
            double maximumPrice = 0;

            if (!values.TryGetValue("buyPrice", out currBuyPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("maximumPrice", out maximumPrice)) throw new VariableNotIncludedException();

            double minimumSellPrice = openPrice * RuleMinimumSellPrice.ProfitFactor;

            double currPriceDeltaPercent = ((currBuyPrice - openPrice) / openPrice) * 100;
            double maximumPriceDeltaPercent = ((maximumPrice - openPrice) / openPrice) * 100;

            if (maximumPriceDeltaPercent < 0 || currPriceDeltaPercent < 0) {
                currentResult = RuleResult.None;
                return;
            }

            double sellPriceTrigger = (maximumPriceDeltaPercent / 4.8571) + 0.5;
            if (sellPriceTrigger > 2) sellPriceTrigger = 2;

            sellPriceTrigger = maximumPriceDeltaPercent - sellPriceTrigger;

            // 0	1
            // 8	2
            // >8	2

            if (currPriceDeltaPercent <= sellPriceTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

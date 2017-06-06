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

            double currPriceDeltaPercent = ((currBuyPrice - minimumSellPrice) / minimumSellPrice) * 100;
            double maximumPriceDeltaPercent = ((maximumPrice - minimumSellPrice) / minimumSellPrice) * 100;

            if (maximumPriceDeltaPercent < 0 || currPriceDeltaPercent < 0) {
                currentResult = RuleResult.None;
                return;
            }

            double sellPriceTrigger = (0.03193 * Math.Pow(maximumPriceDeltaPercent, 2)) + (0.4210 * maximumPriceDeltaPercent) - 0.40336;
            if (maximumPriceDeltaPercent > 10) sellPriceTrigger = maximumPriceDeltaPercent - 2.5;

            // if (maximumPriceDeltaPercent > 15) maximumPriceDeltaPercent = 15; // to lock a minimum of 1% band size on high values
            // double sellPriceTrigger = 0.01071 * Math.Pow(maximumPriceDeltaPercent, 2) + 0.6857 * maximumPriceDeltaPercent - 0.1964;
            // maximum is based on minimumSellPrice (+0.5%)
            // 1        0.5
            // 5        3.5
            // 15      12.5

            if (currPriceDeltaPercent <= sellPriceTrigger)
                currentResult = RuleResult.Sell;
        }
    }
}

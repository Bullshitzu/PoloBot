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

            double priceBandFactor = ((currBuyPrice - minimumSellPrice) / minimumSellPrice) * 100;
            double sellBandSize = ((maximumPrice - minimumSellPrice) / minimumSellPrice) * 100;

            if (sellBandSize < 0 || priceBandFactor < 0) {
                currentResult = RuleResult.None;
                return;
            }

            if (sellBandSize > 17) sellBandSize = 17; // to lock a minimum of 2% band size on high values

            double sellPriceTrigger = (0.04074 * Math.Pow(sellBandSize, 2)) + (0.263 * sellBandSize) - 1.454;
            // 0.04074x^2 + 0.2630x − 1.454
            // that's taking into account the minimumSellPriceFactor
            // It's actually -1, 1, 6
            // -5		-1.75
            // 4		0.25
            // 10		5.25

            if (priceBandFactor <= sellPriceTrigger)
                currentResult = RuleResult.Sell;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleSellBand : TradeRule {

        public static double MaxBandSize = 2.5;

        public static double PriceTriggerMult = 1; //  0.83892454;
        
        public double PriceRiseOffset = 0;

        public override void Recalculate (Dictionary<string, double> values) {

            double currBuyPrice = 0;
            double openPrice = 0;
            double maximumPrice = 0;

            double buyTimestamp = 0;
            double tickerTimestamp = 0;

            // lastBuyTimestamp
            // lastTickerTimestamp

            if (!values.TryGetValue("buyPrice", out currBuyPrice)) throw new VariableNotIncludedException("buyPrice");
            if (!values.TryGetValue("openPrice", out openPrice)) throw new VariableNotIncludedException("openPrice");
            if (!values.TryGetValue("maxPrice", out maximumPrice)) throw new VariableNotIncludedException("maxPrice");

            if (!values.TryGetValue("lastBuyTimestamp", out buyTimestamp)) throw new VariableNotIncludedException("lastBuyTimestamp");
            if (!values.TryGetValue("lastTickerTimestamp", out tickerTimestamp)) throw new VariableNotIncludedException("lastTickerTimestamp");

            double currPriceDeltaPercent = ((currBuyPrice - openPrice) / openPrice) * 100;
            double maximumPriceDeltaPercent = ((maximumPrice - openPrice) / openPrice) * 100;

            if (maximumPriceDeltaPercent < 0 || currPriceDeltaPercent < 0) {
                currentResult = RuleResult.None;
                return;
            }

            double sellPriceTrigger = 0.003339 * (Math.Pow(maximumPriceDeltaPercent, 2)) + 0.04984 * maximumPriceDeltaPercent + 0.8677 + PriceRiseOffset;
            
            if (sellPriceTrigger > MaxBandSize) sellPriceTrigger = MaxBandSize;
            if (sellPriceTrigger < 0) sellPriceTrigger = 0;

            sellPriceTrigger *= PriceTriggerMult;
            sellPriceTrigger = maximumPriceDeltaPercent - sellPriceTrigger;

            // 1.5 = 0.25
            // 5 = 0.5
            // 10 = 1
            // 20 = 2.5

            if (currPriceDeltaPercent < sellPriceTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

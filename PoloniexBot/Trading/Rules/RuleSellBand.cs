using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleSellBand : TradeRule {

        public static double MaxPriceDeltaFactor = 2.74127550;
        public static double SellPriceTriggerOffset = -0.88253461;
        public static double MaxBandSize = 3.045;

        public static double PriceTriggerMult = 0.849;

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

            double sellPriceTrigger = (maximumPriceDeltaPercent / MaxPriceDeltaFactor) + SellPriceTriggerOffset;
            if (sellPriceTrigger > MaxBandSize) sellPriceTrigger = MaxBandSize;
            if (sellPriceTrigger < 0) sellPriceTrigger = 0;

            sellPriceTrigger *= PriceTriggerMult;
            sellPriceTrigger = maximumPriceDeltaPercent - sellPriceTrigger;

            // 1.5 = 0
            // 3 = 0.5
            // 4.5 = 1

            if (currPriceDeltaPercent < sellPriceTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

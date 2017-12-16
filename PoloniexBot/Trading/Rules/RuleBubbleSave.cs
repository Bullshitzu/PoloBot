using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBubbleSave : TradeRule {

        private const double SellTrigger = 5;
        private const double BuyTrigger = 1;

        public static bool BlockTrade = false;

        public override void Recalculate (Dictionary<string, double> values) {

            double MeanRev;

            if (!values.TryGetValue("usdtBtcMRev", out MeanRev)) MeanRev = double.MinValue;

            if (MeanRev > SellTrigger) {
                currentResult = RuleResult.Sell;
                BlockTrade = true;
                return;
            }

            BlockTrade = false;

            if (MeanRev < BuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

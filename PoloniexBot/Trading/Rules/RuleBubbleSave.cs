using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBubbleSave : TradeRule {

        private const double SellTriggerMRev = 4;
        private const double BuyTriggerMRev = 0.5;

        public static bool BlockTrade = false;
        public static bool SellAlts = false;

        public override void Recalculate (Dictionary<string, double> values) {

            double MeanRev;

            if (!values.TryGetValue("usdtBtcMRev", out MeanRev)) throw new VariableNotIncludedException("usdtBtcMRev");

            if (MeanRev > SellTriggerMRev) {
                currentResult = RuleResult.Sell;
                BlockTrade = true;
                SellAlts = true;
                return;
            }

            BlockTrade = false;
            SellAlts = false;

            if (MeanRev < BuyTriggerMRev) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBaseCurrTrend : TradeRule {

        public static double CurrValue = double.MaxValue;
        public static double Trigger = -0.05;

        public override void Recalculate (Dictionary<string, double> values) {

            if (CurrValue < Trigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.BlockBuy;
        }
    }
}

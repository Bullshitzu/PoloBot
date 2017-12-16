using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumBaseAmount : TradeRule {

        public static double MinimumTradeAmount = 0.0001333;
        public static double MinimumAllowedTradeAmount = 0.0001;

        public override void Recalculate (Dictionary<string, double> values) {

            double baseAmount = 0;

            if (!values.TryGetValue("baseAmountTradable", out baseAmount)) throw new VariableNotIncludedException();

            if (baseAmount < MinimumAllowedTradeAmount) {
                currentResult = RuleResult.BlockBuy;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

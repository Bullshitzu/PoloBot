using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumBaseAmount : TradeRule {

        private const double MinimumTradeAmount = 0.0001;

        public override void Recalculate (Dictionary<string, double> values) {

            double baseAmount = 0;

            if (!values.TryGetValue("baseAmountTradable", out baseAmount)) throw new VariableNotIncludedException();

            if (baseAmount < MinimumTradeAmount) {
                currentResult = RuleResult.BlockBuy;
                return;
            }

            currentResult = RuleResult.None;
        }
    }
}

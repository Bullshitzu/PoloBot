using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumBaseAmountOrders : TradeRule {

        public static double MinimumTradeAmount = 0.0001;

        public override void Recalculate (Dictionary<string, double> values) {

            double baseAmountOrders = 0;

            if (!values.TryGetValue("baseAmountOrders", out baseAmountOrders)) throw new VariableNotIncludedException();

            if (baseAmountOrders < MinimumTradeAmount) {
                currentResult = RuleResult.BlockBuy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

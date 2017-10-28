using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumQuoteAmountOrders : TradeRule {

        public static double MinimumTradeAmount = 0.0001;

        public override void Recalculate (Dictionary<string, double> values) {

            double quoteAmountOrders = 0;

            if (!values.TryGetValue("quoteAmountOrders", out quoteAmountOrders)) throw new VariableNotIncludedException();

            if (quoteAmountOrders < MinimumTradeAmount) {
                currentResult = RuleResult.BlockSell;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

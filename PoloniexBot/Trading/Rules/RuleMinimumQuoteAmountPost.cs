using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumQuoteAmountPost : TradeRule {

        private const double MinimumTradeAmount = 0.0001;

        public override void Recalculate (Dictionary<string, double> values) {

            double quoteAmount = 0;

            if (!values.TryGetValue("postQuoteAmount", out quoteAmount)) throw new VariableNotIncludedException();

            if (quoteAmount < MinimumTradeAmount) {
                currentResult = RuleResult.BlockBuy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMinimumQuoteAmount : TradeRule {

        public static double MinimumTradeAmount = 0.0001;

        public override void Recalculate (Dictionary<string, double> values) {
            
            double quoteAmount = 0;

            if (!values.TryGetValue("quoteAmount", out quoteAmount)) throw new VariableNotIncludedException();

            if (quoteAmount < MinimumTradeAmount) {
                currentResult = RuleResult.BlockSell;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

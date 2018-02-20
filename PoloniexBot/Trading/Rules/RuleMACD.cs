using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMACD : TradeRule {

        public override void Recalculate (Dictionary<string, double> values) {

            double shortEMA = 0;
            double longEMA = 0;

            if (!values.TryGetValue("maShort", out shortEMA)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("maLong", out longEMA)) throw new VariableNotIncludedException();

            if (shortEMA > longEMA) {
                currentResult = RuleResult.Buy;
                return;
            }
            if (shortEMA < longEMA) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;
            
        }
    }
}

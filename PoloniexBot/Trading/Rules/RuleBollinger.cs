using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBollinger : TradeRule {

        public override void Recalculate (Dictionary<string, double> values) {

            double price = 0;
            double lowBand = 0;

            if (!values.TryGetValue("buyPrice", out price)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("bollingerBandLow", out lowBand)) throw new VariableNotIncludedException();

            if (price < lowBand) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

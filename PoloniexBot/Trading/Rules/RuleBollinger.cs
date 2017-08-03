using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleBollinger : TradeRule {

        public override void Recalculate (Dictionary<string, double> values) {

            double currPrice = 0;
            double lowerBand = 0;

            if (!values.TryGetValue("buyPrice", out currPrice)) throw new VariableNotIncludedException("buyPrice");
            if (!values.TryGetValue("bollingerBandLow", out lowerBand)) throw new VariableNotIncludedException("bollingerBandLow");

            if (currPrice < lowerBand) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

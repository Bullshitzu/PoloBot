using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMeanRev : TradeRule {

        private const double MeanRevBuyTrigger = 5; // price N% below 3hour mean

        public override void Recalculate (Dictionary<string, double> values) {

            double meanRev = 0;

            if (!values.TryGetValue("meanRev", out meanRev)) throw new VariableNotIncludedException();

            if (meanRev > MeanRevBuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules.Temp {
    class RuleReverse : TradeRule {

        private const double MeanRevTrigger = 5;

        public override void Recalculate (Dictionary<string, double> values) {

            double lowerBand = 0;
            double price = 0;
            double meanRev = 0;

            if (!values.TryGetValue("bollingerBandLower", out lowerBand)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("sellPrice", out price)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("meanRev", out meanRev)) throw new VariableNotIncludedException();
            

            if (lowerBand > price && meanRev > MeanRevTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

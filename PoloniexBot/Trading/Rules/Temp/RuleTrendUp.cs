using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules.Temp {
    class RuleTrendUp : TradeRule {
        
        private const double BandSizeTrigger = 70;
        private const double MeanRevTrigger = -3;

        public override void Recalculate (Dictionary<string, double> values) {
            double bandSizeDelta = 0;
            double meanRev = 0;

            if (!values.TryGetValue("bollingerBandDelta", out bandSizeDelta)) throw new VariableNotIncludedException();
            if (!values.TryGetValue("meanRev", out meanRev)) throw new VariableNotIncludedException();

            if (bandSizeDelta > BandSizeTrigger && meanRev < MeanRevTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

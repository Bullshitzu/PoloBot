using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleVolumeRatio : TradeRule{

        public RuleVolumeRatio (double trigger = 1.5) {
            localTrigger = trigger;
        }

        private double localTrigger = 0;

        public override void Recalculate (Dictionary<string, double> values) {

            double ratio = 0;

            if (!values.TryGetValue("volumeTrend", out ratio)) throw new VariableNotIncludedException();

            if (ratio > localTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;

        }
    }
}

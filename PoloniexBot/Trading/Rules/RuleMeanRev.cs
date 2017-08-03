using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleMeanRev : TradeRule {

        public static double BuyTrigger = 1.57; // price N% below 3hour mean

        public override void Recalculate (Dictionary<string, double> values) {

            double meanRev = 0;
            double meanRevGlobal = 0;

            if (!values.TryGetValue("meanRev", out meanRev)) throw new VariableNotIncludedException("meanRev");
            if (!values.TryGetValue("mRevGlobal", out meanRevGlobal)) throw new VariableNotIncludedException("mRevGlobal");

            if (meanRevGlobal < 0) meanRevGlobal = 0;

            meanRev -= meanRevGlobal * 0.75;

            if (meanRev > BuyTrigger) {
                currentResult = RuleResult.Buy;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

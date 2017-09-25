using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleUSDTBTC : TradeRule {

        public static double Trigger = -0.05;

        public override void Recalculate (Dictionary<string, double> values) {

            double usdtBtcMeanRev;

            if (!values.TryGetValue("usdtBtcMACD", out usdtBtcMeanRev)) throw new VariableNotIncludedException("usdtBtcMACD");

            if (usdtBtcMeanRev < Trigger) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleRSI : TradeRule {

        public static double DefaultTriggerSell = 70;
        public static double DefaultTriggerBuy = 30;

        private double localTriggerSell;
        private double localTriggerBuy;

        public RuleRSI () {
            localTriggerBuy = DefaultTriggerBuy;
            localTriggerSell = DefaultTriggerSell;
        }

        public RuleRSI (double triggerBuy, double triggerSell) {
            localTriggerBuy = triggerBuy;
            localTriggerSell = triggerSell;
        }

        public override void Recalculate (Dictionary<string, double> values) {

            double rsi = 0;

            if (!values.TryGetValue("rsi", out rsi)) throw new VariableNotIncludedException();

            if (rsi < localTriggerBuy) {
                currentResult = RuleResult.Buy;
                return;
            }
            if (rsi > localTriggerSell) {
                currentResult = RuleResult.Sell;
                return;
            }

            currentResult = RuleResult.None;

        }
    }
}

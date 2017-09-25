using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RulePriceDelta : TradeRule {

        public static double Trigger1 = 0.5;
        public static double Trigger2 = 1.5;
        public static double Trigger3 = 3;

        public RulePriceDelta (double trigger = 0.5) {
            localTrigger1 = trigger * 1;
            localTrigger2 = trigger * 3;
            localTrigger3 = trigger * 6;
        }

        private double localTrigger1;
        private double localTrigger2;
        private double localTrigger3;

        public override void Recalculate (Dictionary<string, double> values) {

            double priceDelta1;
            double priceDelta2;
            double priceDelta3;

            if (!values.TryGetValue("priceDelta1", out priceDelta1)) priceDelta1 = double.MaxValue;
            if (!values.TryGetValue("priceDelta2", out priceDelta2)) priceDelta2 = double.MaxValue;
            if (!values.TryGetValue("priceDelta3", out priceDelta3)) priceDelta3 = double.MaxValue;

            if (priceDelta1 > localTrigger1 && priceDelta2 > localTrigger2 && priceDelta3 > localTrigger3) {
                currentResult = RuleResult.Buy;
                return;
            }
            currentResult = RuleResult.None;
        }
    }
}

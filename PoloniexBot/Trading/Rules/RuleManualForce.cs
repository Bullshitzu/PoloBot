using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {
    class RuleManualForce : TradeRule {

        public void Set (RuleResult value) {
            currentResult = value;
        }

        public override void Recalculate (Dictionary<string, double> values) { }
    }
}

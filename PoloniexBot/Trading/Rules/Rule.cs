using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading.Rules {

    public enum RuleResult {
        None,
        Buy,
        Sell,
        BlockBuy,
        BlockSell,
        BlockBuySell,
    }

    public class VariableNotIncludedException : Exception {
        public VariableNotIncludedException () : base("Variable doesn't exist in the dictionary") { }
        public VariableNotIncludedException (string var) : base("Variable doesn't exist in the dictionary: " + var) { }
    }

    abstract class TradeRule {
        internal RuleResult currentResult;
        public RuleResult Result { get { return currentResult; } }
        public abstract void Recalculate (Dictionary<string, double> values);
        public virtual void SetTrigger (params double[] values) {
            // note: most rules don't need to do anything
        }

    }
}

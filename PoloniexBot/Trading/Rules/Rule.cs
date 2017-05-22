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
    }

    abstract class TradeRule {
        internal RuleResult currentResult;
        public RuleResult Result { get { return currentResult; } }
        public abstract void Recalculate (Dictionary<string, double> values);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerPatternMatchBuyTrigger : Optimizer {

        public OptimizerPatternMatchBuyTrigger (double[] vars) : base(vars) { }

        public OptimizerPatternMatchBuyTrigger (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RulePatternMatch.BuyTrigger = val;
        }
    }
}

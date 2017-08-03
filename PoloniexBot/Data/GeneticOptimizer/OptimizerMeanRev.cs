using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMeanRev : Optimizer {

        public OptimizerMeanRev (double[] vars) : base(vars) { }

        public OptimizerMeanRev (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RuleMeanRev.BuyTrigger = val;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMACD : Optimizer {

        public OptimizerMACD (double[] vars) : base(vars) { }

        public OptimizerMACD (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RuleMACD.MacdTrigger = val;
        }
    }
}

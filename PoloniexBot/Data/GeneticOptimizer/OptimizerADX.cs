using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerADX : Optimizer {

        public OptimizerADX (double[] vars) : base(vars) { }

        public OptimizerADX (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RuleADX.Trigger = val;
        }
    }
}

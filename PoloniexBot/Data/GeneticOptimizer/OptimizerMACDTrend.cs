using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMACDTrend : Optimizer {

        public OptimizerMACDTrend (double[] vars) : base(vars) { }

        public OptimizerMACDTrend (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RuleMACDTrend.BuyTrigger = val;
        }

    }
}

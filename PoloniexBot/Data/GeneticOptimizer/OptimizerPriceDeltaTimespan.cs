using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerPriceDeltaTimespan : Optimizer {

        public OptimizerPriceDeltaTimespan (double[] vars) : base(vars) { }

        public OptimizerPriceDeltaTimespan (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Data.Predictors.PriceDelta.Timeframe1 = (long)val;
        }
    }
}

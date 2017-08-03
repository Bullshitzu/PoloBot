using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMeanRevTimespan : Optimizer {

        public OptimizerMeanRevTimespan (double[] vars) : base(vars) { }

        public OptimizerMeanRevTimespan (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Data.Predictors.MeanReversion.MeanTimePeriod = (long)val;
        }
    }
}

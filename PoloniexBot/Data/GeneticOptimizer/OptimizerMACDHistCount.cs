using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMACDHistCount : Optimizer {

        public OptimizerMACDHistCount (double[] vars) : base(vars) { }

        public OptimizerMACDHistCount (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Data.Predictors.MACD.HistogramValueCount = (int)val;
        }

    }
}

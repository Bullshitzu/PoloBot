using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMACDSMA : Optimizer {

        public OptimizerMACDSMA (double[] vars) : base(vars) { }

        public OptimizerMACDSMA (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Data.Predictors.MACD.TimeSMA = (int)val;
        }
    }
}

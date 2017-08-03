using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerMACDEMA : Optimizer {

        public OptimizerMACDEMA (double[] vars) : base(vars) { }

        public OptimizerMACDEMA (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Data.Predictors.MACD.TimeEMA = (int)val;
        }
    }
}

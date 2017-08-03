using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerSellBandFactor : Optimizer {

        public OptimizerSellBandFactor (double[] vars) : base(vars) { }

        public OptimizerSellBandFactor (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RuleSellBand.MaxPriceDeltaFactor = val;
        }

    }
}

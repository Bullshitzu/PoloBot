using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerSellBandOffset : Optimizer {

        public OptimizerSellBandOffset (double[] vars) : base(vars) { }

        public OptimizerSellBandOffset (double startValue, double mutationAmount) : base(startValue, mutationAmount) { }

        public override void SetValue (double val) {
            Trading.Rules.RuleSellBand.SellPriceTriggerOffset = val;
        }

    }
}

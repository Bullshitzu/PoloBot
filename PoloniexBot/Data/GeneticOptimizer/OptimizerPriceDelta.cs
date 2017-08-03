using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public class OptimizerPriceDelta : Optimizer {

        private int variable = 0;

        public OptimizerPriceDelta (double[] vars, int variable) : base(vars) {
            this.variable = variable;
        }

        public OptimizerPriceDelta (double startValue, double mutationAmount, int variable) : base(startValue, mutationAmount) {
            this.variable = variable;
        }

        public override void SetValue (double val) {
            switch (variable) {
                case 1:
                    Trading.Rules.RulePriceDelta.Trigger1 = val;
                    break;
                case 2:
                    Trading.Rules.RulePriceDelta.Trigger2 = val;
                    break;
                case 3:
                    Trading.Rules.RulePriceDelta.Trigger3 = val;
                    break;
                default:
                    break;
            }
        }
    }
}

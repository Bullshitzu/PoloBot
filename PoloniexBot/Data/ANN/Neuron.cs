using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    public class Neuron {

        private double Clamp;
        private double Target;
        private double Factor;

        public Neuron (double clamp, double target = 0) {
            this.Clamp = clamp;
            this.Target = target;
            this.Factor = 1;
        }

        // ----------------------------------

        public double Recalculate (double input) {

            double d = Math.Abs(input - Target);
            d = d / Clamp;

            if (d < 0) d = 0;
            if (d > 1) d = 1;

            d = 1 - (d * 2);
            d = d * Factor;

            return d;
        }




    }
}

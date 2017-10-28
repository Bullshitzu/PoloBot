using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    internal class Synapse {

        public Neuron inputNeuron;
        public double weight;
        public double lastWeight;

        const double MutationChance = 0.3;
        const double MutationWeight = 0.75;

        public Synapse (Neuron inputNeuron) {
            this.inputNeuron = inputNeuron;
            this.lastWeight = 1;
            this.weight = 1;
        }

        public double GetValue () {
            if (inputNeuron == null) return 0;
            return inputNeuron.GetValue() * weight;
        }

        // ------------------------------

        public double GetWeight () {
            return weight;
        }
        public void SetWeight (double val) {
            lastWeight = val;
            weight = val;
        }

        // ------------------------------
        // Genetic optimization
        // ------------------------------

        static Random rand;
        static Synapse () {
            rand = new Random();
        }

        public void Mutate () {
            lastWeight = weight;

            double r = rand.NextDouble();
            if (r <= MutationChance) {
                r = (rand.NextDouble() * 2) - 1; // r < -1, 1 >
                r *= MutationWeight;
                weight += r;
            }
        }
        public void Revert () {
            weight = lastWeight;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    class Synapse {

        static Random rand;
        static Synapse () {
            rand = new Random();
        }

        public Synapse (Neuron inputNeuron) {
            this.inputNeuron = inputNeuron;
            weight = rand.NextDouble(); // * 2 - 1; // -1 to +1
        }

        private const double WeightMax = 1.2;
        private const double WeightMin = -1.2;

        private double oldWeight;
        private double weight;

        private Neuron inputNeuron;

        public double GetValue () {
            return inputNeuron.GetValue() * weight;
        }
        public double GetWeight () {
            return weight;
        }

        public void BackPropagate (double error, double correctionFactor) {

            double gradient = error * weight;
            gradient *= correctionFactor;

            this.weight -= gradient;
            CorrectWeight();

            inputNeuron.BackPropagate(gradient, correctionFactor);
        }

        public void UpdateWeight (double factor) {
            weight *= factor;
        }

        public void Mutate (double factor) {

            oldWeight = weight;
            weight += GetRandomFactor(factor) * weight;

            CorrectWeight();
        }

        public void Revert () {
            weight = oldWeight;
        }

        private static double GetRandomFactor (double mult) {
            return (rand.NextDouble() - 0.5) * 2 * mult;
            // (-1 to +1) * factor
        }

        private void CorrectWeight () {
            if (weight > WeightMax) weight = WeightMax;
            if (weight < WeightMin) weight = WeightMin;
        }
    }
}

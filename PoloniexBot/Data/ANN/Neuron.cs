using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    internal class Neuron {

        private double currValue = 0;
        private Synapse[] inputSynapses;

        internal Neuron (Neuron[] inputNeurons = null) {
            if (inputNeurons == null || inputNeurons.Length == 0) return;

            inputSynapses = new Synapse[inputNeurons.Length];
            for (int i = 0; i < inputSynapses.Length; i++) {
                inputSynapses[i] = new Synapse(inputNeurons[i]);
            }
        }

        public double GetValue () {
            return currValue;
        }
        public void SetValue (double val) {
            currValue = val;
        }

        public void Recalculate () {
            if (inputSynapses == null) return;

            double sum = 0;
            for (int i = 0; i < inputSynapses.Length; i++) {
                sum += inputSynapses[i].GetValue();
            }
            currValue = sum / inputSynapses.Length;

        }

        // ------------------------------

        public void MutateSynapses () {
            if (inputSynapses == null) return;
            for (int i = 0; i < inputSynapses.Length; i++) {
                inputSynapses[i].Mutate();
            }
        }
        public void RevertSynapses () {
            if (inputSynapses == null) return;
            for (int i = 0; i < inputSynapses.Length; i++) {
                inputSynapses[i].Revert();
            }
        }

        // ------------------------------

        public void SetWeights (double[] weights) {
            if (inputSynapses == null) return;
            if (weights.Length != inputSynapses.Length) throw new ArgumentException("Number of weights passed doesn't match the number of input synapses on this Neuron!");
            for (int i = 0; i < inputSynapses.Length; i++) {
                inputSynapses[i].SetWeight(weights[i]);
            }
        }

        public double[] GetWeights () {
            if (inputSynapses == null) return null;
            double[] weights = new double[inputSynapses.Length];
            for (int i = 0; i < inputSynapses.Length; i++) {
                weights[i] = inputSynapses[i].GetWeight();
            }
            return weights;
        }
    }
}

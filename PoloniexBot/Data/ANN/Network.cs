using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    class Network {

        private Neuron[][] Neurons;
        private const double MutateFactor = 0.1;
        private const double CorrectionFactor = 0.8;

        public Network (params int[] layers) {

            Neurons = new Neuron[layers.Length][];
            for (int i = 0; i < layers.Length; i++) {
                Neurons[i] = new Neuron[layers[i]];
            }

            // Generate all neurons and synapses
            for (int i = 0; i < Neurons.Length; i++) {
                for (int j = 0; j < Neurons[i].Length; j++) {
                    Neurons[i][j] = new Neuron();
                    if (i > 0) {
                        // add synapses that link to previous layer
                        for (int z = 0; z < Neurons[i-1].Length; z++) {
                            Synapse s = new Synapse(Neurons[i - 1][z]);
                            Neurons[i][j].AddSynapse(s);
                        }
                    }
                }
            }
        }

        public void SetInputs (double[] values) {
            int inputSize = Neurons[0].Length;
            for (int i = 0; i < Neurons[0].Length; i++) {
                Neurons[0][i].SetValue(values[i]);
            }
        }
        public double[] GetOutputs () {
            int outputSize = Neurons[Neurons.Length - 1].Length;
            double[] data = new double[outputSize];

            for (int i = 0; i < outputSize; i++) {
                data[i] = Neurons[Neurons.Length - 1][i].GetValue();
            }

            return data;
        }

        public void RecalculateNetwork () {
            for (int i = 1; i < Neurons.Length; i++) {
                for (int j = 0; j < Neurons[i].Length; j++) {
                    Neurons[i][j].RecalculateValue();
                }
            }
        }

        public void BackPropagate (double[] errors) {
            int outputIndex = Neurons.Length - 1;
            for (int i = 0; i < Neurons[outputIndex].Length; i++) {
                Neurons[outputIndex][i].BackPropagate(errors[i], CorrectionFactor);
            }
        }

        #region Genetic
        public void Mutate () {
            for (int i = 0; i < Neurons.Length; i++) {
                for (int j = 0; j < Neurons[i].Length; j++) {
                    Neurons[i][j].Mutate(MutateFactor);
                }
            }
        }
        public void RevertMutation () {
            for (int i = 0; i < Neurons.Length; i++) {
                for (int j = 0; j < Neurons[i].Length; j++) {
                    Neurons[i][j].Revert();
                }
            }
        }
        #endregion
    }
}

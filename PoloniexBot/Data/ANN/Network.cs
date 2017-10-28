using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    public class Network : IComparable<Network> {

        private Neuron[][] neurons;
        private double averageError;

        public Network (int[] layers, double averageError) {
            if (layers == null || layers.Length == 0) throw new ArgumentException("Attempt to initialize Network with no Neurons!");

            this.averageError = averageError;

            neurons = new Neuron[layers.Length][];
            for (int i = 0; i < neurons.Length; i++) {
                neurons[i] = new Neuron[layers[i]];
                for (int j = 0; j < neurons[i].Length; j++) {

                    Neuron[] inputNeurons = null;
                    if (i > 0) inputNeurons = neurons[i - 1];

                    neurons[i][j] = new Neuron(inputNeurons);
                }
            }
        }

        public void Recalculate () {
            for (int i = 1; i < neurons.Length; i++) {
                for (int j = 0; j < neurons[i].Length; j++) {
                    neurons[i][j].Recalculate();
                }
            }
        }

        // ------------------------------
        // I/O Handling
        // ------------------------------

        public void SetInputs (double[] inputs, bool ignoreSizeMismatch = false) {
            if(!ignoreSizeMismatch && neurons[0].Length != inputs.Length) throw new ArgumentException("Number of inputs passed doesn't match the number of input Neurons on this Network!");

            int smallerCount = inputs.Length < neurons[0].Length ? inputs.Length : neurons[0].Length;
            for (int i = 0; i < smallerCount; i++) {
                neurons[0][i].SetValue(inputs[i]);
            }
        }
        public double[] GetOutputs () {
            double[] outputs = new double[neurons.Last().Length];
            for (int i = 0; i < outputs.Length; i++) {
                outputs[i] = neurons.Last()[i].GetValue();
            }
            return outputs;
        }

        // ------------------------------
        // Genetic optimization
        // ------------------------------

        public void Mutate () {
            for (int i = 1; i < neurons.Length; i++) {
                for (int j = 0; j < neurons[i].Length; j++) {
                    neurons[i][j].MutateSynapses();
                }
            }
        }
        public void Revert () {
            for (int i = 1; i < neurons.Length; i++) {
                for (int j = 0; j < neurons[i].Length; j++) {
                    neurons[i][j].RevertSynapses();
                }
            }
        }

        // ------------------------------
        // Saving / Parsing
        // ------------------------------

        public string[] ToStringLines () {
            return ToStringLines(averageError);
        }
        public string[] ToStringLines (double averageError) {

            List<string> lines = new List<string>();

            // average error of the network
            lines.Add(averageError.ToString("F8", System.Globalization.CultureInfo.InvariantCulture));
            
            // structure of the network
            //      number of layers
            //      size of layers

            string line = neurons.Length.ToString();
            for (int i = 0; i < neurons.Length; i++) {
                line += ";" + neurons[i].Length.ToString();
            }
            lines.Add(line);

            // synapse states
            //      weights

            for (int i = 1; i < neurons.Length; i++) {
                for (int j = 0; j < neurons[i].Length; j++) {
                    double[] weights = neurons[i][j].GetWeights();

                    line = "";
                    for (int z = 0; z < weights.Length; z++) {
                        line += weights[z].ToString("F8", System.Globalization.CultureInfo.InvariantCulture);
                        if (z + 1 < weights.Length) line += ";";
                    }
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }
        public static Network Parse (string[] lines) {

            // second line = structure

            string line = lines.First();
            double averageError = double.Parse(line, System.Globalization.CultureInfo.InvariantCulture);

            line = lines[1];
            string[] parts = line.Split(';');

            int layerCount = int.Parse(parts[0]);
            int[] layerSizes = new int[layerCount];

            for (int i = 1; i < parts.Length; i++) {
                layerSizes[i - 1] = int.Parse(parts[i]);
            }

            Network tempNet = new Network(layerSizes, averageError);

            // further lines are synapse weights for each neuron

            int lineIndex = 2;
            double[] weights;

            for (int i = 1; i < tempNet.neurons.Length; i++) {
                for (int j = 0; j < tempNet.neurons[i].Length; j++) {

                    line = lines[lineIndex];
                    parts = line.Split(';');

                    weights = new double[parts.Length];
                    for (int z = 0; z < weights.Length; z++) {
                        weights[z] = double.Parse(parts[z], System.Globalization.CultureInfo.InvariantCulture);
                    }

                    tempNet.neurons[i][j].SetWeights(weights);
                    lineIndex++;
                }
            }

            return tempNet;
        }

        // ------------------------------
        // Utility functions
        // ------------------------------

        public int CompareTo (Network other) {
            return this.averageError.CompareTo(other.averageError);
        }
    }
}

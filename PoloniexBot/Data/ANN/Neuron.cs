using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.ANN {
    class Neuron {

        private Synapse[] synapses;
        private double currValue;

        public void RecalculateValue () {
            if (synapses == null) {
                currValue = 0;
                return;
            }

            double valueSum = 0;
            
            for (int i = 0; i < synapses.Length; i++) {
                valueSum += synapses[i].GetValue();
            }

            valueSum /= synapses.Length;
            currValue = valueSum;
        }

        public double GetValue () {
            return currValue;
        }
        public void SetValue (double val) {
            currValue = val;
        }

        public void AddSynapse (Synapse s) {
            List<Synapse> sList;

            if (synapses == null) sList = new List<Synapse>();
            else sList = new List<Synapse>(synapses);

            sList.Add(s);
            synapses = sList.ToArray();
        }

        #region Training
        #region Back-Propagation
        public void BackPropagate (double localError, double correctionFactor) {
            if (synapses == null || synapses.Length == 0) return;
            
            // total output of this neuron needs to change by localError
            // i.e. output+localError would equal optimal output

            // figure out how much each input is responsible for the error
            // since it's a weighted sum, each individual input should equal total output

            for (int i = 0; i < synapses.Length; i++) {
                synapses[i].BackPropagate(localError, correctionFactor);
            }
        }
        #endregion
        #region Genetic
        public void Mutate (double factor) {
            for (int i = 0; i < synapses.Length; i++) {
                synapses[i].Mutate(factor);
            }
        }
        public void Revert () {
            for (int i = 0; i < synapses.Length; i++) {
                synapses[i].Revert();
            }
        }
        #endregion
        #endregion
    }
}

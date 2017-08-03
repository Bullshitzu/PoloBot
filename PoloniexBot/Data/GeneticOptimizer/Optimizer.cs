using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.GeneticOptimizer {
    public abstract class Optimizer {

        public Optimizer (double[] variables) {
            index = 0;
            vars = variables;
        }
        public Optimizer (double startValue, double mutationAmount) {
            this.currValue = startValue;
            this.previousValue = startValue;
            this.mutationAmount = mutationAmount;
        }

        static Optimizer () {
            rand = new Random();
        }
        static System.Random rand;

        internal int index;
        internal double[] vars;

        internal double currValue;
        internal double previousValue;

        internal double mutationAmount;

        private const double MutationChance = 0.3;

        public bool IterateValue () {
            if (vars == null) return true;

            index++;
            if (index >= vars.Length) {
                index = 0;
                return true;
            }

            return false;
        }
        public void SetValue () {
            if (vars == null) return;
            SetValue(vars[index]);
        }

        public abstract void SetValue (double val);

        public int GetIndex () {
            return index;
        }
        public double GetValue () {
            return currValue;
        }

        public void InitializeValue () {
            SetValue(currValue);
        }

        public void Mutate () {
            previousValue = currValue;

            double r = rand.NextDouble();
            if (r > MutationChance) return;

            r = (rand.NextDouble() - 0.5) * 2;
            currValue += mutationAmount * r;
            SetValue(currValue);
        }
        public void Revert () {
            currValue = previousValue;
            SetValue(currValue);
        }

        public override string ToString () {
            return GetType().Name + ": " + (vars[index]);
        }

        public string ToStringSimple () {
            return GetType().Name + ": " + currValue.ToString("F8");
        }


    }
}

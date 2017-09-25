using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data {
    class Optimizer {

        static Optimizer () {
            rand = new Random();
        }
        private static System.Random rand;

        public Optimizer (double startValue, double mutationAmount, SetValueMethod setValueMethod) {
            this.currValue = startValue;
            this.lastValue = startValue;
            this.mutationAmount = mutationAmount;
            this.setValueMethod = setValueMethod;
        }
        public Optimizer (double startValue, double mutationAmount) {
            this.currValue = startValue;
            this.lastValue = startValue;
            this.mutationAmount = mutationAmount;
            this.setValueMethod = null;
        }

        public void SetTranslationMatrix (params double[] values) {
            translationMatrix = values;
        }

        public delegate void SetValueMethod (double value);
        private SetValueMethod setValueMethod;

        private double currValue;
        private double lastValue;

        private double mutationAmount;

        private const double mutationChance = 0.25;

        private double[] translationMatrix;

        public void Mutate (bool lastImproved = false) {
            if (lastImproved) {

                double delta = currValue - lastValue;

                lastValue = currValue;
                currValue += delta;
            }
            else {
                double r = rand.NextDouble();

                if (r < mutationChance) {
                    lastValue = currValue;
                    return;
                }

                r = 1 - (rand.NextDouble() * 2);
                r *= mutationAmount;

                lastValue = currValue;
                currValue += r;
            }
        }
        public void Revert () {
            currValue = lastValue;
        }

        public double GetValue () {
            return Translate(currValue);
        }
        public double GetRawValue () {
            return currValue;
        }
        public void ApplyValues () {
            if (setValueMethod != null) setValueMethod(Translate(currValue));
        }

        // --------------------------

        private double Translate (double value) {
            if (translationMatrix == null) return value;

            int rounded = (int)value;

            if (rounded < 0) {
                double lowerMember = translationMatrix[0];
                double upperMember = translationMatrix[1];

                double d = upperMember - lowerMember;
                return lowerMember - (d * value);

            }
            else if (rounded >= translationMatrix.Length - 1) {
                double lowerMember = translationMatrix[translationMatrix.Length - 2];
                double upperMember = translationMatrix[translationMatrix.Length - 1];

                double d = upperMember - lowerMember;
                return upperMember + (d * (value - (translationMatrix.Length - 1)));
            }
            else {
                double lowerMember = translationMatrix[rounded];
                double upperMember = translationMatrix[rounded + 1];

                double t = value - rounded;
                double d = upperMember - lowerMember;

                return lowerMember + (t * d);

            }
        }

    }
}

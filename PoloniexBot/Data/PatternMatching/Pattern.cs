using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.PatternMatching {
    public class Pattern {

        public double[] movement;

        public Pattern (double[] movement) {
            this.movement = movement;
        }

        public double GetDistance (Pattern other) {

            if (other.movement.Length < this.movement.Length)
                return other.GetDistance(this);

            double errorSum = 0;
            for (int i = 0; i < movement.Length; i++) {
                errorSum += Math.Abs(movement[i] - other.movement[i]);
            }
            return errorSum / movement.Length;
        }

        public override string ToString () {
            string line = movement.Length + ";";
            for (int i = 0; i < movement.Length; i++) {
                line += movement[i].ToString("F8") + ";";
            }
            return line;
        }

        public static Pattern Parse (string text) {
            string[] parts = text.Split(';');

            int movementNum = int.Parse(parts[0]);
            double[] mov = new double[movementNum];

            for (int i = 0; i < movementNum; i++) {
                mov[i] = double.Parse(parts[i + 1]);
            }

            Pattern p = new Pattern(mov);
            return p;
        }

        public class PatternComparer : IComparer<KeyValuePair<Pattern, double>> {
            public int Compare (KeyValuePair<Pattern, double> x, KeyValuePair<Pattern, double> y) {
                return x.Value.CompareTo(y.Value);
            }
        }
    }
}

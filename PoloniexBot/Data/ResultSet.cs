using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data {
    public class ResultSet {

        public struct Variable {

            public string name;
            public double value;
            public int roundCount;
            public bool colorCode;
            public double colorCodeMin;
            public double colorCodeMax;
            public bool showPlusSign;
            public string prefix;
            public string suffix;

            public Variable (string name, double value, int roundCount) {
                this.name = name;
                this.value = value;
                this.roundCount = roundCount;
                colorCode = false;
                colorCodeMin = 0;
                colorCodeMax = 0;
                showPlusSign = false;
                prefix = "";
                suffix = "";
            }
            public Variable (string name, double value, int roundCount, double colorCodeMin, double colorCodeMax) {
                this.name = name;
                this.value = value;
                this.roundCount = roundCount;
                colorCode = true;
                this.colorCodeMin = colorCodeMin;
                this.colorCodeMax = colorCodeMax;
                showPlusSign = false;
                prefix = "";
                suffix = "";
            }
            public Variable (string name, double value, int roundCount, bool showPlusSign, string prefix, string suffix) {
                this.name = name;
                this.value = value;
                this.roundCount = roundCount;
                colorCode = false;
                this.colorCodeMin = 0;
                this.colorCodeMax = 0;
                this.showPlusSign = showPlusSign;
                this.prefix = prefix;
                this.suffix = suffix;
            }
            public Variable (string name, double value, int roundCount, double colorCodeMin, double colorCodeMax, bool showPlusSign, string prefix, string suffix) {
                this.name = name;
                this.value = value;
                this.roundCount = roundCount;
                colorCode = true;
                this.colorCodeMin = colorCodeMin;
                this.colorCodeMax = colorCodeMax;
                this.showPlusSign = showPlusSign;
                this.prefix = prefix;
                this.suffix = suffix;
            }

            public override string ToString () {
                string line = prefix;
                if (showPlusSign && value > 0) line += "+";
                line += value.ToString("F" + roundCount);
                line += suffix;
                return line;
            }

        }

        public long timestamp;
        public string signature;

        public ResultSet (long timestamp) {
            this.timestamp = timestamp;
            this.signature = "[UNSIGNED]";
            variables = new Dictionary<string, Variable>();
        }

        public Dictionary<string, Variable> variables;

    }
}

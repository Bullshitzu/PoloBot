using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.DecisionTree {
    public class Tree {

        private struct DataPointDiscreditized : IComparable<DataPointDiscreditized> {

            public CurrencyPair pair;
            public long timestamp;

            public int result;
            public int[] data;

            public DataPointDiscreditized (CurrencyPair pair, long timestamp, int result, int[] data) {
                this.pair = pair;
                this.timestamp = timestamp;
                this.result = result;
                this.data = data;
            }

            public int CompareTo (DataPointDiscreditized other) {
                return this.result.CompareTo(other.result);
            }

            // ------------------------------------------------------

            public override string ToString () {
                string line = pair + "," + timestamp + "," + result;
                for (int i = 0; i < data.Length; i++) {
                    line += "," + data[i];
                }
                return line;
            }

            public static DataPointDiscreditized Parse (string line) {
                string[] parts = line.Split(',');

                CurrencyPair pair = CurrencyPair.Parse(parts[0]);
                long timestamp = long.Parse(parts[1]);
                int result = int.Parse(parts[2]);

                int[] data = new int[parts.Length - 3];
                for (int i = 3; i < parts.Length; i++) {
                    data[i - 3] = int.Parse(parts[i]);
                }

                return new DataPointDiscreditized(pair, timestamp, result, data);

            }

        }

        public void Train (Precalculation.DataPoint[] data) {

            double[][] breakPoints = FindBreakPoints(data);
            double[] resultBreakPoints = new double[] { -6, -3, 3, 6 };

            DataPointDiscreditized[] dataDiscreditized = Discreditize(data, breakPoints, resultBreakPoints);

            // NOTE: DEBUG

            List<string> lines = new List<string>();

            // write down result break points
            string line = "";
            for (int i = 0; i < resultBreakPoints.Length; i++) {
                line += resultBreakPoints[i].ToString("F2");
                if (i + 1 < resultBreakPoints.Length) line += ",";
            }
            lines.Add(line);

            // write down the number of variable break points
            lines.Add(breakPoints.Length.ToString());

            // write down the variable break points
            for (int i = 0; i < breakPoints.Length; i++) {
                line = "";
                for (int j = 0; j < breakPoints[i].Length; j++) {
                    line += breakPoints[i][j].ToString("F4");
                    if (j + 1 < breakPoints[i].Length) line += ",";
                }
                lines.Add(line);
            }

            // write down the actual data
            for (int i = 0; i < dataDiscreditized.Length; i++) {
                lines.Add(dataDiscreditized[i].ToString());
            }

            Utility.FileManager.SaveFile("data/pcd_" + data.First().Pair + "_discreditized.data", lines.ToArray());

        }
        public double Predict (double[] variables) {

            // todo: this

            return 0;
        }



        public static void AnalyzeDiscreditizedData () {

            DataPointDiscreditized[][] fullData = LoadPrecalculatedData();

            for (int i = 0; i < fullData.Length; i++) {

                float[][] averages = new float[5][];
                int[][] sumCount = new int[5][];

                for (int j = 0; j < averages.Length; j++) {
                    averages[j] = new float[fullData[i].First().data.Length];
                    sumCount[j] = new int[fullData[i].First().data.Length];
                }

                for (int j = 0; j < fullData[i].Length; j++) {
                    for (int k = 0; k < fullData[i][j].data.Length; k++) {
                        averages[fullData[i][j].result][k] += fullData[i][j].data[k];
                        sumCount[fullData[i][j].result][k]++;
                    }
                }

                for (int j = 0; j < averages.Length; j++) {
                    for (int k = 0; k < averages[j].Length; k++) {
                        averages[j][k] /= sumCount[j][k];
                    }
                }

                // now they are actual averages

                string filename = "data/pcda_" + fullData[i].First().pair + ".data";
                List<string> lines = new List<string>();

                for (int j = 0; j < averages.Length; j++) {
                    lines.Add("");
                    lines.Add("Averages for result " + j);
                    for (int k = 0; k < averages[j].Length; k++) {
                        lines.Add(k + ": " + averages[j][k].ToString("F4"));
                    }
                    lines.Add("");
                }

                Utility.FileManager.SaveFile(filename, lines.ToArray());

            }
        }

        private static DataPointDiscreditized[][] LoadPrecalculatedData () {

            string[] files = System.IO.Directory.GetFiles("data");
            if (files == null) return null;

            List<DataPointDiscreditized[]> data = new List<DataPointDiscreditized[]>();

            for (int i = 0; i < files.Length; i++) {
                string filename = files[i].Split('\\', '/').Last();

                if (filename.StartsWith("pcd_")) {
                    Console.WriteLine("Loading file " + filename);

                    string[] lines = Utility.FileManager.ReadFile(files[i]);
                    DataPointDiscreditized[] dataArray = new DataPointDiscreditized[lines.Length];

                    Console.WriteLine("Parsing file " + filename + " (" + lines.Length + " data points)");
                    for (int j = 0; j < lines.Length; j++) {
                        dataArray[j] = DataPointDiscreditized.Parse(lines[j]);
                    }

                    data.Add(dataArray);
                }
            }

            return data.ToArray();
        }

        // ---------------------------------

        private Precalculation.DataPoint[] DiscreditizeData (Precalculation.DataPoint[] data) {
            if (data == null) return null;

            for (int i = 0; i < data.First().Data.Length; i++) {

                double min = double.MaxValue;
                double max = double.MinValue;

                for (int j = 0; j < data.Length; j++) {
                    double currValue = data[j].Data[i];
                    if (currValue < min) min = currValue;
                    if (currValue > max) max = currValue;
                }

                double d = (max - min) / 5;

                double[] borders = new double[5];
                borders[0] = double.MinValue;
                for (int j = 1; j < borders.Length; j++) {
                    borders[j] = min + (d * j);
                }
                
                for (int j = 0; j < data.Length; j++) {
                    double currValue = data[j].Data[i];
                    double endValue = currValue;
                    for (int z = 0; z < borders.Length; z++) {
                        if (currValue > borders[z]) {
                            endValue = min + (z * d) + (d * 0.5);
                        }
                    }
                    data[j].Data[i] = endValue;
                }
            }

            return data;
        }
        private Precalculation.DataPoint[] DiscreditizePrices (Precalculation.DataPoint[] data) {
            if (data == null) return null;

            for (int i = 0; i < data.Length; i++) {
                double currPrice = data[i].Result;

                if (currPrice < -6) currPrice = -6;
                else if (currPrice < -3) currPrice = -3;
                else if (currPrice < 3) currPrice = 0;
                else if (currPrice < 6) currPrice = 3;
                else currPrice = 6;

                data[i].Result = currPrice;
            }

            return data;
        }

        // ---------------------------------

        private double[][] FindBreakPoints (Precalculation.DataPoint[] data) {
            if (data == null) return null;

            double[][] breakPoints = new double[data.First().Data.Length][];

            for (int i = 0; i < data.First().Data.Length; i++) {

                double min = double.MaxValue;
                double max = double.MinValue;

                for (int j = 0; j < data.Length; j++) {
                    double currValue = data[j].Data[i];
                    if (currValue < min) min = currValue;
                    if (currValue > max) max = currValue;
                }

                double d = (max - min) / 5;

                double[] borders = new double[4];
                for (int j = 0; j < borders.Length; j++) {
                    borders[j] = min + (d * (j + 1));
                }

                breakPoints[i] = borders;
            }

            return breakPoints;
        }

        private DataPointDiscreditized[] Discreditize (Precalculation.DataPoint[] data, double[][] varBreakPoints, double[] resultBreakPoints) {
            if (data == null || varBreakPoints == null || resultBreakPoints == null) return null;

            DataPointDiscreditized[] dataDiscreditized = new DataPointDiscreditized[data.Length];

            for (int i = 0; i < data.Length; i++) {

                // discreditize result

                int resultDiscreditized = 0;
                for (int j = 0; j < resultBreakPoints.Length + 1; j++) {
                    if (j < resultBreakPoints.Length && data[i].Result < resultBreakPoints[j]) break;
                    resultDiscreditized = j;
                }

                // discreditize all variables

                int[] varsDiscreditized = new int[data[i].Data.Length];
                for (int j = 0; j < data[i].Data.Length; j++) {
                    for (int z = 0; z < varBreakPoints[j].Length; z++) {
                        if (data[i].Data[j] > varBreakPoints[j][z]) varsDiscreditized[j] = z + 1;
                        else break;
                    }
                }

                dataDiscreditized[i] = new DataPointDiscreditized(data[i].Pair, data[i].Timestamp, resultDiscreditized, varsDiscreditized);

            }

            return dataDiscreditized;
        }

    }
}

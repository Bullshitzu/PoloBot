using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoloniexAPI;

namespace PoloniexBot.Windows.Controls {
    public partial class StrategyScreen : MultiThreadControl {
        public StrategyScreen () {
            InitializeComponent();
        }

        private Dictionary<CurrencyPair, Dictionary<string, double>> variables;
        private KeyValuePair<CurrencyPair, Dictionary<string, double>>[] data;

        public void UpdateData (CurrencyPair pair, Dictionary<string, double> vars) {
            if (this.variables == null) this.variables = new Dictionary<CurrencyPair, Dictionary<string, double>>();

            this.variables.Remove(pair);
            this.variables.Add(pair, vars);

            data = variables.ToArray();
        }

        private string[] drawVariables = { "macd", "meanRev" };

        // ---------------------------------------
        // Drawing
        // ---------------------------------------

        Font fontTitle = new System.Drawing.Font(
                "Calibri Bold Caps", 16F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point,
                ((byte)(238)));
        Brush brush = new SolidBrush(Color.Gray);
        Brush brushEmphasis = new SolidBrush(Color.Silver);
        
        Brush brushUP = new SolidBrush(Color.Green);
        Brush brushDOWN = new SolidBrush(Color.Red);

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Strategy";

            g.Clear(BackColor);

            if (data == null || data.Length == 0) return;

            // 	X X X X
            // 	X X X X
            // 	X X X X

            int index = 0;
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 3; y++) {
                    if (index >= data.Length) return;

                    float xOffset = 5 + (((Width - 10) / 4) * x);
                    float yOffset = 5 + (((Height - 10) / 3) * y);

                    DrawField(g, xOffset, yOffset, data[index].Key, data[index].Value);

                    index++;
                }
            }
        }

        private void DrawField (Graphics g, float xOffset, float yOffset, CurrencyPair pair, Dictionary<string, double> vars) {

            g.DrawString(pair.ToString("/"), fontTitle, brushEmphasis, new PointF(xOffset, yOffset));

            float xPos = xOffset;
            float yPos = yOffset + fontTitle.Height + 5;

            for (int i = 0; i < drawVariables.Length; i++) {
                double tempVar = 0;
                if (vars.TryGetValue(drawVariables[i], out tempVar)) {

                    g.DrawString(drawVariables[i] + ": " + tempVar.ToString("F4"), Font, brush, new PointF(xPos, yPos));
                    yPos += Font.Height + 5;

                }
            }
        }

        private Color GetVariableColor (string varName) {

            int r = Math.Abs(varName.GetHashCode() % 256);
            int g = Math.Abs((r * 2315213) % 256);
            int b = Math.Abs((r * 123523) % 256);

            return Color.FromArgb(r, g, b);
        }
    }
}

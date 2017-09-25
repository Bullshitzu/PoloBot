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

        private List<KeyValuePair<CurrencyPair, Dictionary<string, double>>> data;

        public void UpdateData (CurrencyPair pair, Dictionary<string, double> vars) {
            if (data == null) data = new List<KeyValuePair<CurrencyPair, Dictionary<string, double>>>();

            lock (data) {
                for (int i = 0; i < data.Count; i++) {
                    if (pair == data[i].Key) {
                        data[i] = new KeyValuePair<CurrencyPair, Dictionary<string, double>>(pair, vars);
                        return;
                    }
                }
                data.Add(new KeyValuePair<CurrencyPair, Dictionary<string, double>>(pair, vars));
            }
        }
        public void ClearData () {
            if (data != null) data = null;
        }

        public static string[] drawVariables = { };
        public static double[] minVariables = { };
        public static double[] maxVariables = { };

        // ---------------------------------------
        // Drawing
        // ---------------------------------------

        Font fontTitle = new System.Drawing.Font(
                "Calibri Bold Caps", 16F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point,
                ((byte)(238)));
        Font fontSmall = new System.Drawing.Font(
                "Calibri Bold Caps", 8F,
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

            if (data == null || data.Count == 0) return;

            // 	X X X X
            // 	X X X X
            // 	X X X X

            int index = 0;

            float xMove = (Width - 10) / 5;
            float yMove = (Height- 10) / 4;

            lock (data) {
                for (int x = 0; x < 5; x++) {
                    for (int y = 0; y < 4; y++) {
                        if (index >= data.Count) return;

                        float xOffset = 5 + (xMove * x);
                        float yOffset = 5 + (yMove * y);

                        // DrawField(g, xOffset, yOffset, data[index].Key, data[index].Value);
                        DrawBlock(g, new RectangleF(xOffset, yOffset, xMove, yMove), data[index].Key, data[index].Value);

                        index++;
                    }
                }
            }
        }

        private void DrawBlock (Graphics g, RectangleF rect, CurrencyPair pair, Dictionary<string, double> vars) {

            // quote name on top

            string title = pair.QuoteCurrency;
            float width = g.MeasureString(title, fontSmall).Width;

            g.DrawString(title, fontSmall, brushEmphasis, rect.X + (rect.Width / 2) - (width / 2), rect.Y + 5);

            // variables

            if (vars == null) return;

            float posY = rect.Y + 22;
            using (Brush brushBackground = new SolidBrush(Color.Black)) {
                using (Pen pen = new Pen(brushBackground)) {
                    for (int i = 0; i < drawVariables.Length; i++) {

                        // variable boxes

                        g.FillRectangle(brushBackground, rect.X + 5, posY, rect.Width - 10, 15);

                        // variable bar

                        double var;
                        if (vars.TryGetValue(drawVariables[i], out var)) {
                            double min = minVariables[i];
                            double max = maxVariables[i];

                            double mult = var;
                            if (mult < min) mult = 0.1;
                            else if (mult > max) mult = 1;
                            else mult = (mult - min) / (max - min);

                            Brush barBrush = mult > 0.5 ? brushUP : brushDOWN;

                            g.FillRectangle(barBrush, rect.X + 5, posY, (float)mult * (rect.Width - 10), 15);

                            // variable name

                            g.DrawString(drawVariables[i], fontSmall, brushBackground, new PointF(rect.X + 6, posY + 1));

                        }

                        posY += 20;
                    }

                    // center line

                    g.DrawLine(pen, rect.X + (rect.Width / 2), rect.Y + 20, rect.X + (rect.Width / 2), rect.Y + 80);
                }
            }
        }

        private void DrawField (Graphics g, float xOffset, float yOffset, CurrencyPair pair, Dictionary<string, double> vars) {

            g.DrawString(pair.ToString("/"), fontTitle, brushEmphasis, new PointF(xOffset, yOffset));

            if (vars == null) return;

            float xPos = xOffset;
            float yPos = yOffset + fontTitle.Height;

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

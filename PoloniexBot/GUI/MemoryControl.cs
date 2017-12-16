using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.GUI {
    public class MemoryControl : Templates.BaseControl {

        public MemoryControl () : base() {
            memoryValues = new List<double>();
        }

        private float gridWidth = 15f;
        private float gridHeight = 15f;
        private float gridOffset = 1;

        private float graphMarginX = 45;
        private float graphMarginY = 10;

        private List<double> memoryValues;

        public void UpdateMemoryValue (double val) {
            memoryValues.Add(val);
            while (memoryValues.Count > 51) memoryValues.RemoveAt(0);
        }

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (ClientManager.Training) {
                DrawNoData(g, "DISABLED");
                DrawBorders(g);
                return;
            }

            // Draw grid

            float gridCount = (int)((Width - graphMarginX) / gridWidth);
            float gridSizeX = (Width - graphMarginX) / gridCount;

            gridCount = (int)(Height / gridHeight);
            float gridSizeY = Height / gridCount;

            using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                for (float posX = graphMarginX; posX < Width; posX += gridSizeX) {
                    g.DrawLine(pen, (int)posX, gridOffset, (int)posX, Height);
                }
                for (float posY = gridOffset; posY < Height; posY += gridSizeY) {
                    g.DrawLine(pen, graphMarginX, (int)posY, Width, (int)posY);
                }
            }

            // Find the maximum value
            double maxValue = 0;
            if (memoryValues != null) {
                for (int i = 0; i < memoryValues.Count; i++) {
                    if (memoryValues[i] > maxValue) maxValue = memoryValues[i];
                }
            }
            maxValue *= 1.5;

            // Draw Y labels
            float labelDist = (Height - (2 * graphMarginY)) / 5.8f;
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                for (int i = 0; i < 6; i++) {
                    int val = (int)(((5 - i) / 5f) * maxValue);
                    float posY = graphMarginY + (i * labelDist);

                    string text = (val / 1000000).ToString();
                    float width = g.MeasureString(text, Style.Fonts.Small).Width;

                    g.DrawString(text, Style.Fonts.Small, brush, new PointF(graphMarginX - width - 5, posY));
                }
            }

            // Draw min/max lines
            using (Pen pen = new Pen(Style.Colors.Secondary.Dark2, 3)) {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 2, 2 };

                g.DrawLine(pen, graphMarginX + 5, 8, Width - 5, 8);
                g.DrawLine(pen, graphMarginX + 5, Height - 8, Width - 5, Height - 8);
            }

            // Draw graph values

            Helper.DrawGraphLine(g, new RectangleF(graphMarginX, graphMarginY, Width - graphMarginX, Height - (2 * graphMarginY)),
                memoryValues.ToArray(), maxValue, 0, Style.Colors.Terciary.Dark2, Style.Colors.Terciary.Main, 1.5f);

            // Draw title
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {

                string text = "Memory";
                float width = g.MeasureString(text, Style.Fonts.Title).Width;
                Helper.DrawTextShadow(g, text, new PointF(graphMarginX + 2, 20), Style.Fonts.Title, Color.Black);
                g.DrawString(text, Style.Fonts.Title, brush, new PointF(graphMarginX + 2, 20));

                text = "(mb)";
                Helper.DrawTextShadow(g, text, new PointF(graphMarginX + width + 4, 24), Style.Fonts.Small, Color.Black);
                g.DrawString(text, Style.Fonts.Small, brush, new PointF(graphMarginX + width + 4, 24));
            }


            DrawBorders(g);

        }
    }
}

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
    public class PingControl : Templates.BaseControl {

        public PingControl () : base() {
            pingValues = new List<double>();
        }

        private float gridWidth = 15f;
        private float gridHeight = 15f;
        private float gridOffset = 1;

        private float graphMarginX = 45;
        private float graphMarginY = 10;

        private List<double> pingValues;

        public void UpdatePingValue (double val) {
            pingValues.Add(val);
            while (pingValues.Count > 51) pingValues.RemoveAt(0);
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
            if (pingValues != null) {
                for (int i = 0; i < pingValues.Count; i++) {
                    if (pingValues[i] > maxValue) maxValue = pingValues[i];
                }
            }

            maxValue *= 1.3;

            // Draw Y labels
            float labelDist = (Height - (2 * graphMarginY)) / 5.8f;
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                for (int i = 0; i < 6; i++) {
                    int val = (int)(((5 - i) / 5f) * maxValue);
                    float posY = graphMarginY + (i * labelDist);

                    float width = g.MeasureString(val.ToString(), Style.Fonts.Small).Width;

                    g.DrawString(val.ToString(), Style.Fonts.Small, brush, new PointF(graphMarginX - width - 5, posY));
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
                pingValues.ToArray(), maxValue, 0, Style.Colors.Terciary.Dark2, Style.Colors.Terciary.Main, 1.5f);

            // Draw title
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {

                float width = g.MeasureString("Ping", Style.Fonts.Title).Width;

                Helper.DrawTextShadow(g, "Ping", new PointF(graphMarginX + 2, 20), Style.Fonts.Title, Color.Black);
                g.DrawString("Ping", Style.Fonts.Title, brush, new PointF(graphMarginX + 2, 20));

                Helper.DrawTextShadow(g, "(ms)", new PointF(graphMarginX + width + 4, 24), Style.Fonts.Small, Color.Black);
                g.DrawString("(ms)", Style.Fonts.Small, brush, new PointF(graphMarginX + width + 4, 24));
            }

            // Draw borders
            DrawBorders(g);

        }
    }
}

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
    public partial class ApiCallsControl : Templates.BaseControl {
        public ApiCallsControl () {
            InitializeComponent();
            apiCallValues = new List<double>();
        }

        private float gridWidth = 15f;
        private float gridHeight = 16f;
        private float gridOffset = 1;

        private float graphMarginX = 45;
        private float graphMarginY = 10;

        private List<double> apiCallValues;

        public void UpdateAPICallValue (double val) {
            apiCallValues.Add(val);
            while (apiCallValues.Count > 51) apiCallValues.RemoveAt(0);
        }

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid

            if (ClientManager.Training) {
                DrawNoData(g, "DISABLED");
                DrawBorders(g);
                return;
            }

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
            if (apiCallValues != null) {
                for (int i = 0; i < apiCallValues.Count; i++) {
                    if (apiCallValues[i] > maxValue) maxValue = apiCallValues[i];
                }
            }

            maxValue *= 1.3;

            // Draw Y labels
            float labelDist = (Height - (2 * graphMarginY)) / 5.8f;
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                for (int i = 0; i < 6; i++) {
                    double val = (((5 - i) / 5f) * maxValue);
                    float posY = graphMarginY + (i * labelDist);

                    float width = g.MeasureString(val.ToString("F2"), Style.Fonts.Small).Width;

                    g.DrawString(val.ToString("F2"), Style.Fonts.Small, brush, new PointF(graphMarginX - width - 5, posY));
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
                apiCallValues.ToArray(), maxValue, 0, Style.Colors.Terciary.Dark1, Style.Colors.Terciary.Light2, 1.5f);

            // Draw title
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {

                float width = g.MeasureString("API Calls", Style.Fonts.Title).Width;

                Helper.DrawTextShadow(g, "API Calls", new PointF(graphMarginX + 2, 20), Style.Fonts.Title, Color.Black);
                g.DrawString("API Calls", Style.Fonts.Title, brush, new PointF(graphMarginX + 2, 20));

                Helper.DrawTextShadow(g, "(N/s)", new PointF(graphMarginX + width + 4, 24), Style.Fonts.Small, Color.Black);
                g.DrawString("(N/s)", Style.Fonts.Small, brush, new PointF(graphMarginX + width + 4, 24));
            }

            // Draw smoothed note
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {

                Helper.DrawTextShadow(g, "SMA 5s", new PointF(graphMarginX + 5, Height - Style.Fonts.Reduced.Height - 20), Style.Fonts.Reduced, Color.Black);
                g.DrawString("SMA 5s", Style.Fonts.Reduced, brush, new PointF(graphMarginX + 5, Height - Style.Fonts.Medium.Height - 15));
            }

            // Draw borders
            DrawBorders(g);

        }
    }
}

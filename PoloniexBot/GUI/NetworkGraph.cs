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
    public class NetworkGraph : Templates.BaseControl {

        public NetworkGraph () : base() {
            uploadValues = new List<double>();
            downloadValues = new List<double>();
        }

        private float gridWidth = 15f;
        private float gridHeight = 15f;
        private float gridOffset = 1;

        private float graphMarginX = 45;
        private float graphMarginY = 10;

        private List<double> uploadValues;
        private List<double> downloadValues;

        public void UpdateNetworkValue (long up, long down) {
            uploadValues.Add(up);
            downloadValues.Add(down);

            while (uploadValues.Count > 51) uploadValues.RemoveAt(0);
            while (downloadValues.Count > 51) downloadValues.RemoveAt(0);
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
            if (uploadValues != null) {
                for (int i = 0; i < uploadValues.Count; i++) {
                    if (uploadValues[i] > maxValue) maxValue = uploadValues[i];
                }
            }
            if (downloadValues != null) {
                for (int i = 0; i < downloadValues.Count; i++) {
                    if (downloadValues[i] > maxValue) maxValue = downloadValues[i];
                }
            }

            maxValue *= 1.3;

            string unitPrefix = "";
            int divider = 1;
            int unitDecimals = 0;

            if (maxValue > 1000000) {
                unitPrefix = "M";
                divider = 1000000;
                unitDecimals = 2;
            }
            else if (maxValue > 1000) {
                unitPrefix = "K";
                divider = 1000;
            }

            // Draw Y labels
            float labelDist = (Height - (2 * graphMarginY)) / 5.8f;
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                for (int i = 0; i < 6; i++) {
                    float val = (float)(((5 - i) / 5f) * maxValue);
                    float posY = graphMarginY + (i * labelDist);

                    string text = (val / divider).ToString("F"+unitDecimals);
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

            // Draw borders
            DrawBorders(g);

            // Draw graph values

            Helper.DrawGraphLine(g, new RectangleF(graphMarginX, graphMarginY, Width - graphMarginX - 2, Height - (2 * graphMarginY)),
                uploadValues.ToArray(), maxValue, 0, Style.Colors.Terciary.Dark1, Style.Colors.Terciary.Light2, 1.5f);

            Helper.DrawGraphLine(g, new RectangleF(graphMarginX, graphMarginY, Width - graphMarginX - 2, Height - (2 * graphMarginY)),
                downloadValues.ToArray(), maxValue, 0, Style.Colors.Secondary.Main, Style.Colors.Secondary.Light2, 1.5f);

            // Draw title
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {

                float width = g.MeasureString("Network", Style.Fonts.Title).Width;

                Helper.DrawTextShadow(g, "Network", new PointF(graphMarginX + 2, 20), Style.Fonts.Title, Color.Black);
                g.DrawString("Network", Style.Fonts.Title, brush, new PointF(graphMarginX + 2, 20));

                string text = "(" + unitPrefix + "B/s)";

                Helper.DrawTextShadow(g, text, new PointF(graphMarginX + width + 4, 24), Style.Fonts.Small, Color.Black);
                g.DrawString(text, Style.Fonts.Small, brush, new PointF(graphMarginX + width + 4, 24));
            }

            // Draw legend
            using (Brush brush = new SolidBrush(Style.Colors.Terciary.Light1)) {
                PointF point = new PointF(graphMarginX + 4, Height - graphMarginY - 5 - (Style.Fonts.Small.Height * 2));
                Helper.DrawTextShadow(g, "Upload", point, Style.Fonts.Small, Color.Black);
                g.DrawString("Upload", Style.Fonts.Small, brush, point);
            }
            using (Brush brush = new SolidBrush(Style.Colors.Secondary.Main)) {
                PointF point = new PointF(graphMarginX + 4, Height - graphMarginY - 5 - (Style.Fonts.Small.Height * 1));
                Helper.DrawTextShadow(g, "Download", point, Style.Fonts.Small, Color.Black);
                g.DrawString("Download", Style.Fonts.Small, brush, point);
            }

        }
    }
}

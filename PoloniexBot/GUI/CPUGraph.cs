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
    public class CPUGraph : Templates.BaseControl {

        public CPUGraph () : base() {
            cpuValues = new List<double>();
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text {
            get {
                return base.Text;
            }

            set {
                base.Text = value;
            }
        }

        private float gridWidth = 15f;
        private float gridHeight = 15f;
        private float gridOffset = 1;

        private float graphMarginX = 48;
        private float graphMarginY = 10;

        private List<double> cpuValues;

        public void UpdateCPUValue (double val) {
            cpuValues.Add(val);
            while (cpuValues.Count > 21) cpuValues.RemoveAt(0);
        }

        protected override void OnPaint (System.Windows.Forms.PaintEventArgs e) {
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

            // Draw Y labels
            float labelDist = (Height - (graphMarginY * 2)) / 12f;
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                for (int i = 0; i < 11; i += 2) {
                    int val = 100 - (i * 10);

                    float width = g.MeasureString(val + "%", Style.Fonts.Small).Width;
                    float posY = graphMarginY + (labelDist * i);



                    g.DrawString(val + "%", Style.Fonts.Small, brush, new PointF(graphMarginX - width - 5, posY));
                }
            }

            // Draw min/max lines
            using (Pen pen = new Pen(Style.Colors.Secondary.Dark2, 3)) {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 2, 2 };

                g.DrawLine(pen, graphMarginX + 5, 8, Width - 5, 8);
                g.DrawLine(pen, graphMarginX + 5, Height - 8, Width - 5, Height - 8);
            }

            // Draw label divider
            using (Pen pen = new Pen(Style.Colors.Primary.Main)) {
                g.DrawLine(pen, graphMarginX, graphMarginY, graphMarginX, Height - graphMarginY);
            }

            // Draw cpu values
            Helper.DrawGraphLine(g, new RectangleF(graphMarginX, graphMarginY, Width - graphMarginX, Height - (2 * graphMarginY)),
                cpuValues.ToArray(), 100, 0, Style.Colors.Terciary.Dark2, Style.Colors.Terciary.Main, 1.5f);


            // Draw title
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                Helper.DrawTextShadow(g, Text, new PointF(graphMarginX + 2, 14), Style.Fonts.Title, Color.Black);
                g.DrawString(Text, Style.Fonts.Title, brush, new PointF(graphMarginX + 2, 14));
            }

            // Finally, draw the borders of the control
            DrawBorders(g);
        }

    }
}

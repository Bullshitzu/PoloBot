using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.GUI.Templates {
    public partial class BaseControl : UserControl {
        public BaseControl () {
            InitializeComponent();

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);
        }

        const int BorderOffset = 1;

        protected override void OnPaint (PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            DrawBorders(g);
        }

        protected void DrawNoData (Graphics g, string text = "NO DATA") {
            
            Helper.DrawRectangleFancy1(g, new RectangleF(Width / 2 - 50, Height / 2 - 25, 100, 50), Style.Colors.Primary.Main, 2);

            float width = g.MeasureString(text, Style.Fonts.Medium).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                g.DrawString(text, Style.Fonts.Medium, brush, (Width / 2) - (width / 2), (Height / 2) - (Style.Fonts.Medium.Size / 2));
            }
        }
        protected void DrawNoDataSmall (Graphics g, RectangleF rect) {

            Helper.DrawRectangleFancy1(g, new RectangleF(rect.X +15, rect.Y+15, rect.Width-30, rect.Height-30), Style.Colors.Primary.Main, 2);

            string s = "NO DATA";
            float width = g.MeasureString(s, Style.Fonts.Small).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                g.DrawString(s, Style.Fonts.Small, brush, rect.X + (rect.Width/2) - (width / 2), rect.Y + (rect.Height/2) - (Style.Fonts.Medium.Size / 2));
            }
        }

        protected void DrawBorders (Graphics g, float thickness = 2) {
            DrawBorders(g, Style.Colors.Primary.Main, thickness);
        }
        protected void DrawBorders (Graphics g, Color color, float thickness = 2) {
            using (Pen pen = new Pen(color, thickness)) {
                g.DrawLine(pen, BorderOffset, BorderOffset, Width - BorderOffset, BorderOffset);
                g.DrawLine(pen, BorderOffset, BorderOffset, BorderOffset, Height - BorderOffset);
                g.DrawLine(pen, Width - BorderOffset, BorderOffset, Width - BorderOffset, Height - BorderOffset);
                g.DrawLine(pen, BorderOffset, Height - BorderOffset, Width - BorderOffset, Height - BorderOffset);
            }
        }
    }
}

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
    public partial class StatusControl : Templates.BaseControl {
        public StatusControl () {
            InitializeComponent();
        }

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float width = 0;

            // Draw System text

            width = g.MeasureString("ENVIRONMENT: ", Style.Fonts.Title).Width;
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light2)) {
                g.DrawString("ENVIRONMENT: ", Style.Fonts.Title, brush, new Point(5, 7));
            }

            Color environmentColor;
            switch (GUIManager.environment) {
                case GUIManager.Environment.Development:
                    environmentColor = Style.Colors.Terciary.Main;
                    break;
                case GUIManager.Environment.Simulation:
                    environmentColor = Style.Colors.Primary.Main;
                    break;
                case GUIManager.Environment.Live:
                    environmentColor = Style.Colors.Secondary.Main;
                    break;
                default:
                    environmentColor = Style.Colors.Terciary.Main;
                    break;
            }

            using (Brush brush = new SolidBrush(environmentColor)) {
                g.DrawString(GUIManager.environment.ToString().ToUpper(), Style.Fonts.Title, brush, new PointF(7 + width, 7));
            }

            // Draw active market count
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                string activeMarkets = "Active Market: Poloniex";
                g.DrawString(activeMarkets, Style.Fonts.Small, brush, new PointF(5, Height - 5 - Style.Fonts.Small.Height));
            }

            // Draw version text
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                string build = "Build: 1774";
                width = g.MeasureString(build, Style.Fonts.Tiny).Width;
                g.DrawString(build, Style.Fonts.Tiny, brush, new PointF(Width - width - 5, Height - 5 - Style.Fonts.Tiny.Height));
            }

            // Finally, draw the borders of the control
            DrawBorders(g);

        }
    }
}

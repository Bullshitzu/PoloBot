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
    public class ThreadsControl : Templates.BaseControl {

        string[] groupNames = { "Core", "Exchange", "Network", "Pairs" };
        string[][] groupValues = {
            new string[] { "Update TP", "+ TP", "- TP", "Replace TP", "Clear TP" },
            new string[] { "Monitor", "Pull Trades", "Up. Curr.", "Up. Market", "Up. Wallet" },
            new string[] { "Public", "Trade", "Parse", "Ping" },
        };

        public int ActiveThreadCount = 0;
        public string[] ActivePairs = null;

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float width = 0;

            float posX = 0;
            float posY = 0;

            // Draw title
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                g.DrawString("Active Modules", Style.Fonts.Title, brush, new PointF(7, 7));
            }

            // Draw total threads label
            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                string totalThreads = "Active Threads: " + ActiveThreadCount;
                width = g.MeasureString(totalThreads, Style.Fonts.Small).Width;
                g.DrawString(totalThreads, Style.Fonts.Small, brush, new PointF(Width - width - 5, 7));
            }

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {

                    posY = 33;

                    for (int i = 0; i < groupNames.Length; i++) {
                        string text = groupNames[i];

                        width = g.MeasureString(text, Style.Fonts.Small).Width;
                        g.DrawString(text, Style.Fonts.Small, brush, new PointF(40 - (width / 2), posY + 3));

                        posX = 78;

                        string[] source = i < groupValues.Length ? groupValues[i] : ActivePairs;
                        if (source == null) continue;

                        for (int j = 0; j < source.Length; j++) {

                            width = g.MeasureString(source[j], Style.Fonts.Tiny).Width + 10;

                            int xSize = (int)(width / 69) + 1;
                            int gapCount = xSize;
                            xSize = xSize * 69;

                            if (posX + xSize - 5 > Width) {
                                posX = 78;
                                posY += 25;
                            }

                            g.DrawRectangle(pen, posX, posY, xSize - 5, 20);
                            Helper.DrawRectangleFancy1(g, new RectangleF(posX, posY, xSize - 5, 20), Style.Colors.Primary.Dark1, 1);

                            g.DrawString(source[j], Style.Fonts.Tiny, brush, posX + (xSize / 2) - (width / 2) + 4, posY + 4);

                            Color threadCol = Style.Colors.Primary.Dark1;
                            if (i == 2 && j == 3) threadCol = Style.Colors.Secondary.Dark1;
                            if (i == 3 && (j == 2 || j == 6)) threadCol = Style.Colors.Secondary.Dark1;

                            posX += xSize;
                        }

                        posY += 33;

                    }
                }
            }

            // Draw borders
            DrawBorders(g);

        }
    }
}

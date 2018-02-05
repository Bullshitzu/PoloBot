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
    public class MainSummaryGraph : Templates.BaseControl {

        private float gridWidth = 25f;
        private float gridHeight = 25f;
        private float gridOffset = 1;

        private float graphMarginX = 1;
        private float graphMarginY = 1;

        private float rightDivider = 56;
        private float bottomDivider = 27;

        private const long GraphTimeframe = 93600; // 26 hours

        public bool MarkedBorder = false;

        public enum NetworkMessageState {
            Hide,
            Down,
            Restored,
        }

        public NetworkMessageState NetworkMessage = NetworkMessageState.Hide;

        public void UpdateTickers (PoloniexAPI.TickerChangedEventArgs[] newTickers) {
            tickers = newTickers;
        }

        PoloniexAPI.TickerChangedEventArgs[] tickers;

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            try {
                if (tickers != null) {

                    float posY = 0;

                    // Draw grid

                    float gridCount = (int)((Width - graphMarginX - rightDivider) / gridWidth);
                    float gridSizeX = (Width - graphMarginX - rightDivider) / gridCount;

                    gridCount = (int)((Height - bottomDivider) / gridHeight);
                    float gridSizeY = (Height - bottomDivider) / gridCount;

                    using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                        for (float posX = graphMarginX; posX < Width - rightDivider; posX += gridSizeX) {
                            g.DrawLine(pen, (int)posX + 1, gridOffset, (int)posX + 1, Height - bottomDivider);
                        }
                        for (posY = gridOffset; posY < Height - bottomDivider; posY += gridSizeY) {
                            g.DrawLine(pen, graphMarginX, (int)posY, Width - rightDivider, (int)posY);
                        }

                        // Draw dividers

                        g.DrawLine(pen, graphMarginX, Height - bottomDivider, Width, Height - bottomDivider);
                        g.DrawLine(pen, Width - rightDivider, gridOffset, Width - rightDivider, Height);

                    }

                    // Draw min/max lines
                    using (Pen pen = new Pen(Style.Colors.Secondary.Dark2, 3)) {
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        pen.DashPattern = new float[] { 2, 2 };

                        g.DrawLine(pen, graphMarginX + 2, gridSizeY / 2, Width - rightDivider - 2, gridSizeY / 2);
                        g.DrawLine(pen, graphMarginX + 2, Height - bottomDivider - (gridSizeY / 2), Width - rightDivider - 2, Height - bottomDivider - (gridSizeY / 2));
                    }

                    // Draw timeframe legend
                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        g.DrawString("(-t)", Style.Fonts.Tiny, brush, new PointF(Width - rightDivider + 5, Height - bottomDivider + 5));
                    }

                    // Find the minimum and maximum value
                    double maxValue = 0;
                    double minValue = double.MaxValue;

                    for (int i = 0; i < tickers.Length; i++) {
                        if (tickers[i].MarketData.OrderTopBuy < minValue) minValue = tickers[i].MarketData.OrderTopBuy;
                        if (tickers[i].MarketData.OrderTopSell > maxValue) maxValue = tickers[i].MarketData.OrderTopSell;
                    }

                    // Draw price legend

                    int legendCount = (int)gridCount - 1;
                    float legendHeight = (Height - bottomDivider - (2 * gridSizeY)) / legendCount;

                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        for (int i = 0; i < legendCount; i++) {
                            double price = ((maxValue - minValue) / (legendCount - 1)) * i + minValue;
                            posY = Height - bottomDivider - gridSizeY - (gridSizeY * i) - 6;

                            g.DrawString(((int)price).ToString(), Style.Fonts.Tiny, brush, Width - rightDivider + 10, posY);

                        }
                    }

                    // Draw timeframe labels
                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        int number = 0;
                        for (float x = Width - rightDivider - gridSizeX * 3; x > 0; x -= gridSizeX * 3) {
                            number += 180;
                            int hours = number / 60;
                            int mins = number % 60;

                            string tText = hours.ToString("D2") + ":" + mins.ToString("D2");
                            tText = tText.Trim();
                            float width = g.MeasureString(tText, Style.Fonts.Tiny).Width;

                            g.DrawString(tText, Style.Fonts.Tiny, brush, new PointF(x - (width / 2), Height - bottomDivider + 5));
                        }
                    }

                    // Draw graph
                    Helper.DrawGraphLine(g, new RectangleF(graphMarginX + 2, graphMarginY + gridSizeY,
                        Width - (graphMarginX * 2) - rightDivider, Height - (graphMarginY * 2) - bottomDivider - (gridSizeY * 2)),
                        tickers, maxValue, minValue, tickers.Last().Timestamp - GraphTimeframe, 104, 1.5f);

                    // Draw title
                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {

                        string text = "USDT / BTC";

                        float width = g.MeasureString(text, Style.Fonts.Title).Width;

                        Helper.DrawTextShadow(g, text, new PointF(graphMarginX + 6, 30), Style.Fonts.Title, Color.Black);
                        g.DrawString(text, Style.Fonts.Title, brush, new PointF(graphMarginX + 6, 30));
                    }

                    // Draw current value

                    using (Brush brush = new SolidBrush(Style.Colors.Terciary.Dark1)) {

                        string text = tickers.Last().MarketData.PriceLast.ToString("F2");

                        float width = g.MeasureString(text, Style.Fonts.Title).Width;

                        Helper.DrawTextShadow(g, text, new PointF(graphMarginX + 6, 57), Style.Fonts.Title, Color.Black);
                        g.DrawString(text, Style.Fonts.Title, brush, graphMarginX + 6, 57);

                    }

                    // Draw meanRev and stDev
                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {

                        string text = "M.Rev.: " + Trading.Strategies.BaseTrendMonitor.LastUSDTBTCMeanRev.ToString("F4");
                        float width = g.MeasureString(text, Style.Fonts.Title).Width;
                        Helper.DrawTextShadow(g, text, new PointF(graphMarginX + 6, Height - bottomDivider - 27 - Style.Fonts.Title.Height), Style.Fonts.Title, Color.Black);
                        g.DrawString(text, Style.Fonts.Title, brush, new PointF(graphMarginX + 6, Height - bottomDivider - 27 - Style.Fonts.Title.Height));

                        text = "A.D.X.: " + Trading.Strategies.BaseTrendMonitor.LastUSDTBTCADX.ToString("F");
                        width = g.MeasureString(text, Style.Fonts.Title).Width;
                        Helper.DrawTextShadow(g, text, new PointF(graphMarginX + 6, Height - bottomDivider - 50 - Style.Fonts.Title.Height), Style.Fonts.Title, Color.Black);
                        g.DrawString(text, Style.Fonts.Title, brush, new PointF(graphMarginX + 6, Height - bottomDivider - 50 - Style.Fonts.Title.Height));
                    }

                    // Draw Network Down / Restored

                    RectangleF networkMessageRect = new RectangleF(Width * 0.3f, Height * 0.25f, Width * 0.4f, Height * 0.4f);
                    switch (NetworkMessage) {
                        case NetworkMessageState.Hide:
                            // do nothing
                            break;
                        case NetworkMessageState.Down:
                            DrawNetworkDown(g, networkMessageRect);
                            break;
                        case NetworkMessageState.Restored:
                            DrawNetworkRestored(g, networkMessageRect);
                            break;
                    }
                }
                else DrawNoData(g);

                // Draw borders
                DrawBordersCustom(g, (MarkedBorder ? Style.Colors.Terciary.Dark1 : Style.Colors.Primary.Main));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message + " - " + ex.StackTrace);
            }
        }

        private void DrawBordersCustom (Graphics g, Color color) {
            using (Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, 1, 1, Width - 1, 1);
                g.DrawLine(pen, 1, 1, 1, Height - 1);
                g.DrawLine(pen, Width - 1, 1, Width - 1, Height - 1);
                g.DrawLine(pen, 1, Height - 1, Width - 1, Height - 1);
            }
        }

        private void DrawNetworkRestored (Graphics g, RectangleF rect) {
            DrawNetworkMessage(g, rect, "NETWORK RESTORED", Style.Colors.Primary.Main);
        }

        private void DrawNetworkDown (Graphics g, RectangleF rect) {
            DrawNetworkMessage(g, rect, "NETWORK DOWN", Style.Colors.Secondary.Dark1);
        }

        private void DrawNetworkMessage (Graphics g, RectangleF rect, string message, Color color) {

            // fill background
            using (Brush brush = new SolidBrush(Style.Colors.Background)) {
                g.FillRectangle(brush, rect);
            }

            // draw borders
            using (Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }

            // draw text
            float width = g.MeasureString(message, Style.Fonts.Title).Width;

            using (Brush brush = new SolidBrush(color)) {
                g.DrawString(message, Style.Fonts.Title, brush, rect.X + (rect.Width / 2) - (width / 2), rect.Y + (rect.Height / 2) - (Style.Fonts.Title.Height / 2));
            }
        }
    }
}

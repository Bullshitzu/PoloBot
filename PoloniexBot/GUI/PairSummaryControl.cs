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
    public class PairSummaryControl : Templates.BaseControl {


        private float gridWidth = 15f;
        private float gridHeight = 15f;
        private float gridOffset = 1;

        private float gridRightDivider = 10;

        private float graphMarginX = 150;
        private float graphMarginY = 10;

        // --------------------------

        public PoloniexAPI.CurrencyPair pair;

        private double lastPrice = 0;
        private double change24 = 0;
        private double volume = 0;

        public bool MarkedUser = false; // manually marked by user
        public bool Blocked = false;

        private PoloniexAPI.TickerChangedEventArgs[] priceData;

        private const int GraphTimeframe = 6;

        // --------------------------

        public void UpdatePair (PoloniexAPI.CurrencyPair pair, PoloniexAPI.TickerChangedEventArgs[] tickers, bool marked) {
            if (pair == null) return;
            if (tickers == null || tickers.Length == 0) return;

            this.pair = pair;
            this.MarkedUser = marked;

            lock (this) {
                priceData = tickers;
            }

            this.lastPrice = tickers.Last().MarketData.PriceLast;
            this.change24 = tickers.Last().MarketData.PriceChangePercentage * 100;
            this.volume = tickers.Last().MarketData.Volume24HourBase;
        }

        public void SetToNoData () {
            lock (this) {
                pair = null;
                MarkedUser = false;
                priceData = null;
            }
        }

        public void SetBlocked (bool state) {
            this.Blocked = state;
        }

        // --------------------------

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float posY = 0;

            if (ClientManager.Training) {
                DrawNoData(g, "DISABLED");
                DrawBorders(g);
                return;
            }
            
            try {
                lock (this) {
                    if (priceData != null) {

                        // Draw grid

                        float rightDivider = Width - gridRightDivider;

                        float gridCount = (int)((rightDivider - graphMarginX) / gridWidth);
                        float gridSizeX = (rightDivider - graphMarginX) / gridCount;



                        gridCount = (int)(Height / gridHeight);
                        float gridSizeY = Height / gridCount;

                        using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                            for (float posX = graphMarginX; posX < rightDivider; posX += gridSizeX) {
                                g.DrawLine(pen, (int)posX, gridOffset, (int)posX, Height);
                            }
                            for (posY = gridOffset; posY < Height; posY += gridSizeY) {
                                g.DrawLine(pen, graphMarginX, (int)posY, rightDivider, (int)posY);
                            }
                            g.DrawLine(pen, rightDivider, gridOffset, rightDivider, Height);
                        }

                        Color pairNameColor;

                        if (Blocked) pairNameColor = Style.Colors.Secondary.Dark1;
                        else if (MarkedUser) pairNameColor = Style.Colors.Terciary.Main;
                        else pairNameColor = Style.Colors.Primary.Light1;

                        // ------------------

                        // Draw pair name
                        using (Brush brush = new SolidBrush(pairNameColor)) {
                            g.DrawString(pair.BaseCurrency + " / " + pair.QuoteCurrency, Style.Fonts.Medium, brush, new PointF(6, 7));
                        }
                        posY = Style.Fonts.Medium.Height + 10;

                        // Draw price
                        float width = 0;
                        string[] priceParts = Helper.SplitLeadingZeros(lastPrice.ToString("F8"));

                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                            width = g.MeasureString("Price: ", Style.Fonts.Tiny).Width;
                            g.DrawString("Price: ", Style.Fonts.Tiny, brush, new PointF(7, posY));
                        }
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Dark1)) {
                            g.DrawString(priceParts[0], Style.Fonts.Tiny, brush, new PointF(7 + width, posY));
                            width += g.MeasureString(priceParts[0], Style.Fonts.Tiny).Width;
                        }
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                            g.DrawString(priceParts[1], Style.Fonts.Tiny, brush, new PointF(5 + width, posY));
                        }
                        posY += Style.Fonts.Tiny.Height + 3;

                        // Draw volume
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                            width = g.MeasureString("Volume: ", Style.Fonts.Tiny).Width;
                            g.DrawString("Volume: ", Style.Fonts.Tiny, brush, new PointF(7, posY));
                        }
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                            g.DrawString(volume.ToString("F3"), Style.Fonts.Tiny, brush, new PointF(7 + width, posY));
                        }
                        posY += Style.Fonts.Tiny.Height + 3;

                        // Draw 24h change
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                            width = g.MeasureString("Change: ", Style.Fonts.Tiny).Width;
                            g.DrawString("Change: ", Style.Fonts.Tiny, brush, new PointF(7, posY));
                        }

                        Color changeColor = Helper.LerpColor((float)change24, -30, 30, Style.Colors.Negative, Style.Colors.Positive);

                        string changeString = "";
                        if (change24 > 0) changeString += "+";
                        changeString += change24.ToString("F2") + "%";

                        using (Brush brush = new SolidBrush(changeColor)) {
                            g.DrawString(changeString, Style.Fonts.Tiny, brush, new PointF(5 + width, posY));
                        }

                        // Draw min/max lines
                        using (Pen pen = new Pen(Style.Colors.Secondary.Dark2, 3)) {
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                            pen.DashPattern = new float[] { 2, 2 };

                            g.DrawLine(pen, graphMarginX + 5, 8, rightDivider - 5, 8);
                            g.DrawLine(pen, graphMarginX + 5, Height - 8, rightDivider - 5, Height - 8);
                        }

                        double maxValue = 0;
                        double minValue = double.MaxValue;

                        // Find the minimum and maximum value
                        for (int i = 0; i < priceData.Length; i++) {
                            if (priceData[i].MarketData.PriceLast > maxValue) maxValue = priceData[i].MarketData.PriceLast;
                            if (priceData[i].MarketData.PriceLast < minValue) minValue = priceData[i].MarketData.PriceLast;
                        }

                        maxValue *= 1.001;
                        minValue *= 0.999;

                        // Draw graph data
                        Helper.DrawGraphLine(g, new RectangleF(graphMarginX, graphMarginY, rightDivider - graphMarginX, Height - (graphMarginY * 2)),
                            priceData.ToArray(), maxValue, minValue, priceData.Last().Timestamp - (GraphTimeframe * 3600), 38, 1.5f);

                        // Draw spread
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                            PointF point = new PointF(graphMarginX + 4, Height - graphMarginY - 5 - Style.Fonts.Small.Height);
                            float graphSpread = (float)((maxValue - minValue) / minValue) * 100;
                            Helper.DrawTextShadow(g, "Spread: " + graphSpread.ToString("F4") + "%", point, Style.Fonts.Small, Color.Black);
                            g.DrawString("Spread: " + graphSpread.ToString("F4") + "%", Style.Fonts.Small, brush, point);
                        }

                        // Draw legend
                        using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                            PointF point = new PointF(graphMarginX + 4, Height - graphMarginY - 10 - (Style.Fonts.Small.Height * 2));

                            string text = "Graph: " + GraphTimeframe + "h";
                            Helper.DrawTextShadow(g, text, point, Style.Fonts.Small, Color.Black);
                            g.DrawString(text, Style.Fonts.Small, brush, point);
                        }
                    }
                    else DrawNoData(g);
                }

                DrawBorders(g);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message + " - " + ex.StackTrace);
            }
        }
    }
}

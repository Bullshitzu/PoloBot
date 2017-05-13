using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoloniexAPI;
using PoloniexAPI.MarketTools;
using Utility;

namespace PoloniexBot.Windows.Controls {
    public partial class ChartScreen : MultiThreadControl {
        public ChartScreen () {
            InitializeComponent();
        }

        IList<IMarketChartData> chartData;

        CurrencyPair selectedPair;
        MarketPeriod period;

        float gridXOffset = 0;
        double pricePosMult = 0;

        Brush brushGridVolume = new SolidBrush(Color.FromArgb(23, 42, 44));
        Brush brushCandleGreen = new SolidBrush(Color.FromArgb(17, 126, 26));
        Brush brushCandleRed = new SolidBrush(Color.FromArgb(123, 17, 17));
        Brush brushText = new SolidBrush(Color.FromArgb(107, 144, 148));
        Pen PenTextOutline = new Pen(new SolidBrush(Color.FromArgb(16, 16, 16)), 5);
        Brush brushOrders = new SolidBrush(Color.FromArgb(39, 65, 65));

        Font font;
        Font fontMedium = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        Font fontLarge = new System.Drawing.Font("Calibri Bold Caps", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

        public void UpdateChartData (IList<IMarketChartData> data, CurrencyPair pair, MarketPeriod period) {
            if (chartData != null) {
                lock (chartData) {
                    chartData = data;
                }
            }
            else chartData = data;
            selectedPair = pair;
            this.period = period;
        }

        void DrawTextShadow (Graphics g, string text, Font font, Pen pen, Brush brush, float xPos, float yPos) {

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (System.Drawing.Drawing2D.GraphicsPath path = GetStringPath(text, g.DpiY, font, new PointF(xPos, yPos))) {
                g.DrawPath(pen, path);
                g.FillPath(brush, path);
            }

            g.SmoothingMode = oldSmoothingMode;
        }
        System.Drawing.Drawing2D.GraphicsPath GetStringPath (string text, float dpi, Font font, PointF point) {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            float emSize = font.Size * 1.38f;
            path.AddString(text, font.FontFamily, (int)font.Style, emSize, point, StringFormat.GenericTypographic);
            return path;
        }

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Chart";

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.Clear(this.BackColor);

            font = this.Font;

            if (chartData == null) return;


            float leftBorder = 5;
            float rightBorder = Width - 5;
            float rightGridBorder = rightBorder - 60;

            float topBorder = 5;
            float bottomBorder = Height - 5;

            double maxPrice = double.MinValue;
            double minPrice = double.MaxValue;
            double maxVolume = double.MinValue;

            lock (chartData) {
                for (int i = 0; i < chartData.Count; i++) {
                    if (chartData[i].High > maxPrice) maxPrice = chartData[i].High;
                    if (chartData[i].Low < minPrice) minPrice = chartData[i].Low;
                    if (chartData[i].VolumeBase > maxVolume) maxVolume = chartData[i].VolumeBase;
                }
            }

            double spread = maxPrice - minPrice;

            #region Basic Chart

            // -----------------------------
            // Horizontal Lines
            // -----------------------------

            double t = (int)DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now) % (int)period;
            t /= (int)period;

            int candleCount = (int)(DateTimeHelper.DateTimeToUnixTimestamp(chartData.Last().Time) - DateTimeHelper.DateTimeToUnixTimestamp(chartData.First().Time));
            double timeDiff = candleCount;
            candleCount /= (int)period;

            Pen linePen = new Pen(brushGridVolume, 1);
            Pen thickLinePen = new Pen(brushText, 2);
            Pen ordersPen = new Pen(brushOrders, 2);

            float gridXSize = ((rightGridBorder - leftBorder) / candleCount) * 4;
            float gridYSize = (bottomBorder - topBorder) / 10f;
            float bottomGridBorder = bottomBorder - gridYSize;

            gridXOffset = (float)(t * gridXSize);

            for (float i = topBorder + gridYSize; i < bottomBorder; i += gridYSize) {
                g.DrawLine(linePen, new PointF(leftBorder, i), new PointF(rightBorder, i));
            }

            // --------------------------------
            // Prices Text
            // --------------------------------

            double priceDiff = (maxPrice - minPrice) / 8;
            double priceYStart = bottomGridBorder - font.Height - 1;

            for (int i = 0; i < 9; i++) {
                g.DrawString((minPrice + (i * priceDiff)).ToString("F6"), font, brushText, (float)(rightGridBorder + 2), (float)(priceYStart - i * gridYSize));
            }

            // --------------------------------
            // Data
            // --------------------------------

            double volumeMult = (1 / maxVolume) * (bottomGridBorder - topBorder) * 0.75f;
            double candleXPosMult = (rightGridBorder - leftBorder - 7) / (candleCount + 1);

            float posYStart = bottomGridBorder - 1;

            pricePosMult = ((maxPrice - minPrice) / spread) * (gridYSize * 8);

            lock (chartData) {
                for (int i = 0; i < chartData.Count; i++) {

                    float posX = leftBorder + 5f + (float)(candleXPosMult * i);

                    if (posX - 3.5f < leftBorder) continue;
                    if (posX + 3.5f > rightGridBorder) continue;

                    bool isPeriod = chartData[i].Time.Minute % 15 == 0;

                    if (isPeriod) {

                        g.DrawLine(linePen, new PointF(posX, topBorder), new PointF(posX, bottomBorder));

                        if (posX - 7 > leftBorder && posX + 7 < rightGridBorder) {

                            string time = chartData[i].Time.Hour + ":" + chartData[i].Time.Minute.ToString("D2");
                            string date = chartData[i].Time.Day + "." + chartData[i].Time.Month;

                            float width = g.MeasureString(time, font).Width;
                            g.DrawString(time, font, brushText, posX - (width / 2), bottomBorder - 27);

                            width = g.MeasureString(date, font).Width;
                            g.DrawString(date, font, brushText, posX - (width / 2), bottomBorder - 15);
                        }
                    }

                    float volumeHeight = (float)(chartData[i].VolumeBase * volumeMult);
                    g.FillRectangle(brushGridVolume, posX - 3, posYStart - volumeHeight, 7, volumeHeight);

                    // high / low are lines

                    double lowPos = (chartData[i].Low - minPrice) / spread;
                    lowPos *= pricePosMult;
                    lowPos = priceYStart - lowPos;

                    double highPos = (chartData[i].High - minPrice) / spread;
                    highPos *= pricePosMult;
                    highPos = priceYStart - highPos;

                    g.DrawLine(ordersPen,
                        new PointF(posX, (float)lowPos),
                        new PointF(posX, (float)highPos));

                    // open / close are candles

                    double openPos = (chartData[i].Open - minPrice) / spread;
                    openPos *= pricePosMult;
                    openPos = priceYStart - openPos;

                    double closePos = (chartData[i].Close - minPrice) / spread;
                    closePos *= pricePosMult;
                    closePos = priceYStart - closePos;

                    if (openPos > closePos) g.FillRectangle(brushCandleGreen, posX - 3.5f, (float)closePos, 7, (float)(openPos - closePos));
                    else g.FillRectangle(brushCandleRed, posX - 3.5f, (float)openPos, 7, (float)(closePos - openPos));

                }
            }

            // --------------------------------
            // Edges
            // --------------------------------

            g.DrawLine(thickLinePen, new PointF(leftBorder, topBorder), new PointF(leftBorder, bottomBorder));

            g.DrawLine(thickLinePen, new PointF(rightGridBorder, topBorder), new PointF(rightGridBorder, bottomBorder));
            g.DrawLine(thickLinePen, new PointF(rightBorder, topBorder), new PointF(rightBorder, bottomBorder));

            g.DrawLine(thickLinePen, new PointF(leftBorder, bottomBorder), new PointF(rightBorder, bottomBorder));
            g.DrawLine(thickLinePen, new PointF(leftBorder, topBorder), new PointF(rightBorder, topBorder));

            g.DrawLine(thickLinePen, new PointF(leftBorder, bottomGridBorder), new PointF(rightBorder, bottomGridBorder));

            #endregion

            // --------------------------------
            // Pair Text
            // --------------------------------

            DrawTextShadow(g, selectedPair.BaseCurrency + " / " + selectedPair.QuoteCurrency, fontLarge, PenTextOutline, brushText, leftBorder + 10, topBorder + 5);
            DrawTextShadow(g, "Graph: 6 hours", fontMedium, PenTextOutline, brushText, leftBorder + 10, bottomGridBorder - 10 - fontMedium.Height - fontMedium.Height);
            DrawTextShadow(g, "Period: 5 minutes", fontMedium, PenTextOutline, brushText, leftBorder + 10, bottomGridBorder - 10 - fontMedium.Height);

        }
    }
}

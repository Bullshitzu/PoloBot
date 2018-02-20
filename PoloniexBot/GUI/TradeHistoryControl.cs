using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utility.Log;
using Utility;

namespace PoloniexBot.GUI {
    public partial class TradeHistoryControl : Templates.BaseControl {
        public TradeHistoryControl () {
            InitializeComponent();
        }

        public Utility.TSList<TradeTracker.TradeData> openPositions = null;
        public Utility.TSList<TradeTracker.TradeData> closedPositions = null;
        public Utility.TSList<Utility.Log.MessageTypes.Message> basicMessages = null;
        public Utility.TSList<Utility.Log.MessageTypes.ErrorMessage> errorMessages = null;

        private float gridWidth = 25f;
        private float gridHeight = 25f;

        private int positionShowCount = 10;
        private float positionBoxMargin = 5;

        int topDivider = 0;
        int bottomDivider = 0;
        int centerDivider = 0;
        int rightDivider = 0;

        public long chartEndTime = 0;
        long chartTimespan = 182400; // 48 hours + 2 hours + 40 minutes

        protected override void OnPaint (PaintEventArgs e) {

            try {

                Graphics g = e.Graphics;
                g.Clear(Style.Colors.Background);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Set the dividers

                topDivider = Height / 4;
                bottomDivider = Height - (Height / 10);
                centerDivider = topDivider + (bottomDivider - topDivider) / 2;
                rightDivider = Width - (Width / 18);

                // Draw grid

                float gridCount = (int)(rightDivider / gridWidth);
                float gridSizeX = rightDivider / gridCount;

                gridCount = (int)((topDivider - bottomDivider) / gridHeight);
                float gridSizeY = (topDivider - bottomDivider) / gridCount;

                using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                    for (float posX = 1; posX < rightDivider; posX += gridSizeX) {
                        // if (posX == 1) continue;
                        g.DrawLine(pen, (int)posX, topDivider, (int)posX, bottomDivider);
                    }
                    for (float posY = topDivider; posY < bottomDivider; posY += gridSizeY) {
                        g.DrawLine(pen, 1, (int)posY, rightDivider - 1, (int)posY);
                    }
                }

                // Draw divider lines
                using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                    g.DrawLine(pen, 1, topDivider, Width - 1, topDivider);
                    g.DrawLine(pen, 1, bottomDivider, Width - 1, bottomDivider);
                    g.DrawLine(pen, rightDivider, topDivider, rightDivider, Height);
                }
                using (Pen pen = new Pen(Style.Colors.Primary.Dark1)) {
                    g.DrawLine(pen, 1, centerDivider, rightDivider - 1, centerDivider);
                }

                // Draw percent delta labels
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                    g.DrawString("0%", Style.Fonts.Small, brush, new PointF(rightDivider + 5,
                        centerDivider - (Style.Fonts.Small.Height / 2)));

                    g.DrawString("+5%", Style.Fonts.Small, brush, new PointF(rightDivider + 5,
                        topDivider + (centerDivider - topDivider) * 0.67f - (Style.Fonts.Small.Height / 2)));
                    g.DrawString("-5%", Style.Fonts.Small, brush, new PointF(rightDivider + 5,
                        topDivider + (centerDivider - topDivider) * 1.35f - (Style.Fonts.Small.Height / 2)));

                    g.DrawString("+10%", Style.Fonts.Small, brush, new PointF(rightDivider + 5,
                        topDivider + (centerDivider - topDivider) * 0.337f - (Style.Fonts.Small.Height / 2)));
                    g.DrawString("-10%", Style.Fonts.Small, brush, new PointF(rightDivider + 5,
                        topDivider + (centerDivider - topDivider) * 1.69f - (Style.Fonts.Small.Height / 2)));
                }

                // Draw dump line and label

                float dumpLinePosX = rightDivider - (gridSizeX * 4.5f) + 1;
                using (Pen pen = new Pen(Style.Colors.Primary.Dark2, 2)) {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    pen.DashPattern = new float[] { 1, 2 };

                    g.DrawLine(pen, dumpLinePosX, topDivider, dumpLinePosX, bottomDivider);
                }
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                    string text = "DMP";
                    float width = g.MeasureString(text, Style.Fonts.Tiny).Width;

                    g.DrawString(text, Style.Fonts.Tiny, brush, dumpLinePosX - (width / 2) + 1, bottomDivider - Style.Fonts.Tiny.Height - 12);
                }

                // Draw min/max lines
                using (Pen pen = new Pen(Style.Colors.Secondary.Dark2, 3)) {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    pen.DashPattern = new float[] { 2, 2 };

                    g.DrawLine(pen, 7, topDivider + 8, rightDivider - 5, topDivider + 8);
                    g.DrawLine(pen, 7, bottomDivider - 9, rightDivider - 5, bottomDivider - 9);
                }

                // Draw timeframe legend
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                    g.DrawString("-t(h)", Style.Fonts.Tiny, brush, new PointF(rightDivider + 5, bottomDivider + 5));
                }

                // Draw timeframe labels
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                    int number = 0;
                    for (float y = rightDivider - gridSizeX * 3; y > 0; y -= gridSizeX * 3) {
                        number += 240;
                        int hours = number / 60;
                        int mins = number % 60;

                        string tText = hours.ToString("D2") + ":" + mins.ToString("D2");
                        tText = tText.Trim();
                        float width = g.MeasureString(tText, Style.Fonts.Tiny).Width;

                        g.DrawString(tText, Style.Fonts.Tiny, brush, new PointF(y - (width / 2) + 1, bottomDivider + 5));
                    }
                }

                if (openPositions != null && closedPositions != null) {

                    // Draw the open and closed positions
                    int openCount = 0;
                    if (openPositions != null) openCount = openPositions.Count;

                    int closedCount = 0;
                    if (closedPositions != null) closedCount = closedPositions.Count;

                    int maxClosed = 0;
                    if (openCount >= positionShowCount) maxClosed = 0;
                    else maxClosed = positionShowCount - openCount;

                    maxClosed = closedPositions.Count - maxClosed;
                    if (maxClosed < 0) maxClosed = 0;

                    // Draw closed
                    float PosX = positionBoxMargin + 2;
                    int posWidth = (int)(Width - (2 * positionBoxMargin) - ((positionShowCount - 1) * positionBoxMargin)) / positionShowCount;

                    for (int i = maxClosed; i < closedCount; i++) {
                        DrawClosedPosition(g, closedPositions[i], PosX, posWidth);
                        PosX += posWidth + positionBoxMargin;
                    }

                    // Draw open
                    for (int i = 0; i < openCount; i++) {
                        DrawOpenPosition(g, openPositions[i], PosX, posWidth);
                        PosX += posWidth + positionBoxMargin;
                    }

                    // Draw the graph symbols
                    if (closedPositions != null) {
                        for (int i = 0; i < closedPositions.Count; i++) {
                            DrawGraphClose(g, closedPositions[i]);
                        }
                    }
                    if (openPositions != null) {
                        for (int i = 0; i < openPositions.Count; i++) {
                            DrawGraphOpen(g, openPositions[i]);
                        }
                    }
                }

                // Draw messages
                if (basicMessages != null) {
                    for (int i = 0; i < basicMessages.Count; i++) {
                        if (basicMessages[i] == null) continue;
                        if (!basicMessages[i].DrawInHistory) continue;
                        DrawBasicMessage(g, basicMessages[i]);
                    }
                }
                if (errorMessages != null) {
                    for (int i = 0; i < errorMessages.Count; i++) {
                        if (errorMessages[i] == null) continue;
                        if (!errorMessages[i].DrawInHistory) continue;
                        DrawErrorMessage(g, errorMessages[i]);
                    }
                }

                // Draw title
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {

                    string text = "History";

                    float width = g.MeasureString(text, Style.Fonts.Title).Width;

                    Helper.DrawTextShadow(g, text, new PointF(7, topDivider + 18), Style.Fonts.Title, Color.Black);
                    g.DrawString(text, Style.Fonts.Title, brush, new PointF(7, topDivider + 18));
                }

                // Draw the legend
                using (Brush brush = new SolidBrush(Color.Black)) {
                    g.FillEllipse(brush, 43, bottomDivider - 52, 18, 18);
                    g.FillEllipse(brush, 50, bottomDivider - 34, 18, 18);
                }
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                    Helper.DrawTextShadow(g, "Open:", new PointF(7, bottomDivider - 50), Style.Fonts.Small, Color.Black);
                    Helper.DrawTextShadow(g, "Close:", new PointF(7, bottomDivider - 32), Style.Fonts.Small, Color.Black);

                    g.DrawString("Open:", Style.Fonts.Small, brush, new PointF(7, bottomDivider - 50));
                    g.DrawString("Close:", Style.Fonts.Small, brush, new PointF(7, bottomDivider - 32));

                    g.FillEllipse(brush, 45, bottomDivider - 50, 14, 14);
                    g.FillEllipse(brush, 52, bottomDivider - 32, 14, 14);
                }
                using (Brush brush = new SolidBrush(Color.Black)) {
                    g.FillEllipse(brush, 47, bottomDivider - 48, 10, 10);
                }

                // Finally, draw the borders
                DrawBorders(g);

            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message+" - "+ex.StackTrace);
            }
        }

        // drawing messages

        private void DrawBasicMessage (Graphics g, Utility.Log.MessageTypes.Message m) {
            
            long currTimestamp = chartEndTime;

            float posXChart = (currTimestamp - Utility.DateTimeHelper.DateTimeToUnixTimestamp(m.Time)) * ((float)rightDivider / chartTimespan);
            posXChart = rightDivider - posXChart;

            if (posXChart < 0) return;

            using (Brush brush = new SolidBrush(Style.Colors.Terciary.Dark2)) {
                g.FillEllipse(brush, posXChart - 5, centerDivider - 5, 10, 10);

                float width = g.MeasureString(m.Text, Style.Fonts.Tiny).Width;
                g.DrawString(m.Text, Style.Fonts.Tiny, brush, posXChart - (width / 2), centerDivider - 8 - Style.Fonts.Tiny.Height);
            }
        }

        private void DrawErrorMessage (Graphics g, Utility.Log.MessageTypes.ErrorMessage m) {
            
            long currTimestamp = chartEndTime;

            float posXChart = (currTimestamp - Utility.DateTimeHelper.DateTimeToUnixTimestamp(m.Time)) * ((float)rightDivider / chartTimespan);
            posXChart = rightDivider - posXChart;

            if (posXChart < 0) return;

            using (Pen pen = new Pen(Style.Colors.Secondary.Dark1, 2)) {
                g.DrawLine(pen, posXChart - 5, centerDivider, posXChart + 5, centerDivider);
                g.DrawLine(pen, posXChart, centerDivider - 5, posXChart, centerDivider + 5);
            }
        }

        // drawing open / closed positions

        private void DrawClosedPosition (Graphics g, TradeTracker.TradeData cp, float posX, float posWidth) {

            string text = "";
            float width = 0;
            float textPos = 0;
            string[] partsOpen = Helper.SplitLeadingZeros(cp.buyPrice.ToString("F" + cp.displayDigits));
            string[] partsClose = Helper.SplitLeadingZeros(cp.sellPrice.ToString("F" + cp.displayDigits));

            float posXOpen = 0;
            float posXClose = 0;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Dark2)) {

                // Draw open price (leading zeros)
                text = cp.buyPrice.ToString("F" + cp.displayDigits);
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth / 2) - (width / 2) + posX - 3;
                posXOpen = textPos;

                g.DrawString(partsOpen[0], Style.Fonts.Tiny, brush, new PointF(textPos, 10));
                posXOpen += g.MeasureString(partsOpen[0], Style.Fonts.Tiny).Width;


                // Draw close price (leading zeros)
                text = cp.sellPrice.ToString("F" + cp.displayDigits);
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth / 2) - (width / 2) + posX - 3;
                posXClose = textPos;

                g.DrawString(partsClose[0], Style.Fonts.Tiny, brush, new PointF(textPos, 25));
                posXClose += g.MeasureString(partsClose[0], Style.Fonts.Tiny).Width;
            }

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {

                // Draw open price (remainder)
                g.DrawString(partsOpen[1], Style.Fonts.Tiny, brush, new PointF(posXOpen - 2, 10));

                // Draw close price (remainder)
                g.DrawString(partsClose[1], Style.Fonts.Tiny, brush, new PointF(posXOpen - 2, 25));

                // Draw currency name
                text = cp.pair.QuoteCurrency == "BTC" ? cp.pair.BaseCurrency : cp.pair.QuoteCurrency;
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth / 4) - (width / 2) - 4;
                g.DrawString(text, Style.Fonts.Tiny, brush, new PointF(posX + textPos, 40));

            }

            // Draw percent change

            Color percentColor = Helper.LerpColor((float)cp.percentGain, -5, 5, Style.Colors.Negative, Style.Colors.Positive);
            using (Brush brush = new SolidBrush(percentColor)) {
                text = (cp.percentGain > 0 ? "+" : "") + cp.percentGain.ToString("F3") + "%";
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth * 0.67f) - (width / 2) - 3;
                g.DrawString(text, Style.Fonts.Tiny, brush, new PointF(posX + textPos, 40));
            }

            using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                g.DrawRectangle(pen, posX, positionBoxMargin, posWidth, topDivider - (2 * positionBoxMargin));
            }

            DrawGraphPosition(g, cp.buyTimestamp, posX + (posWidth / 2), Style.Colors.Primary.Dark1);
        }
        private void DrawOpenPosition (Graphics g, TradeTracker.TradeData op, float posX, float posWidth) {

            string text = "";
            float width = 0;
            float textPos = 0;
            string[] partsOpen = Helper.SplitLeadingZeros(op.buyPrice.ToString("F" + op.displayDigits));
            string[] partsCurr = Helper.SplitLeadingZeros(op.openPrice.ToString("F" + op.displayDigits));

            float posXOpen = 0;
            float posXClose = 0;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Dark2)) {

                // Draw open price (leading zeros)
                text = op.buyPrice.ToString("F" + op.displayDigits);
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth / 2) - (width / 2) + posX - 3;
                posXOpen = textPos;

                g.DrawString(partsOpen[0], Style.Fonts.Tiny, brush, new PointF(textPos, 10));
                posXOpen += g.MeasureString(partsOpen[0], Style.Fonts.Tiny).Width;

            }

            using (Brush brush = new SolidBrush(Style.Colors.Terciary.Dark2)) {

                // Draw current price (leading zeros)
                text = op.openPrice.ToString("F" + op.displayDigits);
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth / 2) - (width / 2) + posX - 3;
                posXClose = textPos;

                g.DrawString(partsCurr[0], Style.Fonts.Tiny, brush, new PointF(textPos, 25));
                posXClose += g.MeasureString(partsCurr[0], Style.Fonts.Tiny).Width;
            }

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {

                // Draw open price (remainder)
                g.DrawString(partsOpen[1], Style.Fonts.Tiny, brush, new PointF(posXOpen - 2, 10));

                // Draw currency name
                text = op.pair.QuoteCurrency == "BTC" ? op.pair.BaseCurrency : op.pair.QuoteCurrency;
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth / 4) - (width / 2) - 4;
                g.DrawString(text, Style.Fonts.Tiny, brush, new PointF(posX + textPos, 40));

            }

            using (Brush brush = new SolidBrush(Style.Colors.Terciary.Main)) {

                // Draw current price (remainder)
                g.DrawString(partsCurr[1], Style.Fonts.Tiny, brush, new PointF(posXClose - 2, 25));
            }

            // Draw percent change

            Color percentColor = Helper.LerpColor((float)op.percentGain, -5f, 5, Style.Colors.Negative, Style.Colors.Positive);
            using (Brush brush = new SolidBrush(percentColor)) {
                text = (op.percentGain > 0 ? "+" : "") + op.percentGain.ToString("F3") + "%";
                width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                textPos = (posWidth * 0.67f) - (width / 2) - 3;
                g.DrawString(text, Style.Fonts.Tiny, brush, new PointF(posX + textPos, 40));
            }

            using (Pen pen = new Pen(Style.Colors.Terciary.Dark2)) {
                g.DrawRectangle(pen, posX, positionBoxMargin, posWidth, topDivider - (2 * positionBoxMargin));
            }

            DrawGraphPosition(g, op.buyTimestamp, posX + (posWidth / 2), Style.Colors.Terciary.Dark2, true);
        }

        private void DrawGraphPosition (Graphics g, long timestamp, float posX, Color color, bool drawOutsideGraph = false) {

            // posX is the panel position (above the graph)

            long currTimestamp = chartEndTime;
            
            float posXChart = (currTimestamp - timestamp) * ((float)rightDivider / chartTimespan);
            posXChart = rightDivider - posXChart;

            if (drawOutsideGraph && posXChart < 15) posXChart = 15;
            else if (!drawOutsideGraph && posXChart < 0) return;

            using (Pen pen = new Pen(color, 2)) {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 3, 4 };

                g.DrawLine(pen, posX, topDivider - 5, posXChart, centerDivider);
            }
        }

        private void DrawGraphOpen (Graphics g, TradeTracker.TradeData op) {

            long currTimestamp = chartEndTime;
            float posXChart = (currTimestamp - op.buyTimestamp) * ((float)rightDivider / chartTimespan);
            posXChart = rightDivider - posXChart;

            if (posXChart < 15) posXChart = 15;

            // find the yPos of the current price

            float ySize = bottomDivider - topDivider;
            float yPosCurr = ((float)op.percentGain / 30f) * -ySize + centerDivider;
            if (yPosCurr < topDivider + 5) yPosCurr = topDivider + 5;
            if (yPosCurr > bottomDivider - 5) yPosCurr = bottomDivider - 5;

            using (Pen pen = new Pen(Style.Colors.Terciary.Dark2, 2)) {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 3, 4 };

                g.DrawLine(pen, posXChart, centerDivider, rightDivider, yPosCurr);
            }

            // draw stop loss icons
            if (op.stopLossPercent > 0.875) {
                float posYStopLoss = (((float)(op.stopLossPercent - 1) * 100) / 30) * -ySize + centerDivider;
                using (Pen pen = new Pen(Style.Colors.Secondary.Dark2)) {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    pen.DashPattern = new float[] { 3, 4 };

                    g.DrawLine(pen, posXChart, posYStopLoss, rightDivider, posYStopLoss);
                    g.DrawLine(pen, posXChart, centerDivider, posXChart, posYStopLoss);
                }
                using (Brush brush = new SolidBrush(Style.Colors.Secondary.Dark1)) {
                    g.FillEllipse(brush, posXChart - 3, posYStopLoss - 3, 6, 6);
                    g.FillEllipse(brush, rightDivider - 3, posYStopLoss - 3, 6, 6);
                }
            }

            // draw the icons and currency name

            using (Brush brush = new SolidBrush(Style.Colors.Terciary.Main)) {
                g.FillEllipse(brush, posXChart - 7, centerDivider - 7, 14, 14);
                g.FillEllipse(brush, rightDivider - 4, yPosCurr - 4, 8, 8);

                string text = op.pair.QuoteCurrency == "BTC" ? op.pair.BaseCurrency : op.pair.QuoteCurrency;

                float width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                g.DrawString(text, Style.Fonts.Tiny, brush, new PointF(posXChart - (width / 2) + 2, centerDivider + 10));
            }
            using (Brush brush = new SolidBrush(Style.Colors.Background)) {
                g.FillEllipse(brush, posXChart - 5, centerDivider - 5, 10, 10);
                g.FillEllipse(brush, rightDivider - 2, yPosCurr - 2, 4, 4);
            }
        }
        private void DrawGraphClose (Graphics g, TradeTracker.TradeData cp) {

            long currTimestamp = chartEndTime;
            float posXChartOpen = (currTimestamp - cp.buyTimestamp) * ((float)rightDivider / chartTimespan);
            float posXChartClose = (currTimestamp - cp.sellTimestamp) * ((float)rightDivider / chartTimespan);
            posXChartOpen = rightDivider - posXChartOpen;
            posXChartClose = rightDivider - posXChartClose;

            if (posXChartClose < 0) return;

            // find the yPos of the current price

            float ySize = bottomDivider - topDivider;
            float yPosCurr = ((float)cp.percentGain/ 30f) * -ySize + centerDivider;
            if (yPosCurr < topDivider + 5) yPosCurr = topDivider + 5;
            if (yPosCurr > bottomDivider - 5) yPosCurr = bottomDivider - 5;

            using (Pen pen = new Pen(Style.Colors.Primary.Dark1, 2)) {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 3, 4 };

                g.DrawLine(pen, posXChartOpen, centerDivider, posXChartClose, yPosCurr);
            }

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                g.FillEllipse(brush, posXChartOpen - 7, centerDivider - 7, 14, 14);
                g.FillEllipse(brush, posXChartClose - 5, yPosCurr - 5, 10, 10);

                string text = cp.pair.QuoteCurrency == "BTC" ? cp.pair.BaseCurrency : cp.pair.QuoteCurrency;

                float width = g.MeasureString(text, Style.Fonts.Tiny).Width;
                g.DrawString(text, Style.Fonts.Tiny, brush, new PointF(posXChartOpen - (width / 2) + 2, centerDivider + 10));
            }

            using (Brush brush = new SolidBrush(Style.Colors.Background)) {
                g.FillEllipse(brush, posXChartOpen - 5, centerDivider - 5, 10, 10);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class Volume : Predictor {

        const int Period = 60;
        const int PeriodInterval = 10;

        static int[] Settings = { 30, 300 };

        private double avgTradesPerSecond = 0;
        private long[] timespans = { 120, 7200 }; // these don't really matter, they're overwritten in Setup (calculated per pair individually)

        public Volume (CurrencyPair pair) : base(pair) { }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Volume";
        }

        public void Setup (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            long timescale = tickers.Last().Timestamp - tickers.First().Timestamp;
            avgTradesPerSecond = tickers.Length / (double)timescale;

            long tsShort = (long)((1 / avgTradesPerSecond) * Settings[0] + 1); // note: time in seconds for 10 trades (average)
            long tsLong = (long)((1 / avgTradesPerSecond) * Settings[1] + 1); // note: time in seconds for 75 trades (average)

            timespans = new long[] { tsShort, tsLong };
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            long currTime = tickers.Last().Timestamp;
            ResultSet rs = new ResultSet(currTime);

            double volumeShort = GetVolume(tickers, timespans[0]) / timespans[0];
            double volumeLong = GetVolume(tickers, timespans[1]) / timespans[1];
            double deltaVolume = 0;

            rs.variables.Add("volumeShort", new ResultSet.Variable("Short Volume", volumeShort, 3));
            rs.variables.Add("volumeLong", new ResultSet.Variable("Long Volume", volumeLong, 3));

            if (results != null && results.Count > 5) {
                ResultSet.Variable lastRs;
                double sum = 0;

                for (int i = results.Count-1; i >= 0 && i > results.Count-6; i--) {
                    if (results[i].variables.TryGetValue("volumeShort", out lastRs)) sum += lastRs.value;
                    else {
                        // todo: some kind of error notification
                    }
                }

                deltaVolume = sum / 5;
                deltaVolume = volumeShort / deltaVolume;
            }

            rs.variables.Add("volumeDelta", new ResultSet.Variable("Delta Volume", deltaVolume, 3));

            SaveResult(rs);
        }

        static double GetVolume (TickerChangedEventArgs[] tickers, long timespan) {
            if (tickers == null || tickers.Length == 0) return 0;

            long lastTimestamp = tickers.Last().Timestamp;
            long firstTimestamp = lastTimestamp - timespan;

            double sum = 0;

            float mult = 1f / (lastTimestamp - firstTimestamp);

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < firstTimestamp) break;

                float factor = (lastTimestamp - tickers[i].Timestamp) * mult;
                factor = 1 - factor;
                sum += factor;
            }

            return sum;
        }

        // -------------------
        // Drawing Variables
        // -------------------

        private Color colorShort = Color.FromArgb(128, 128, 255);
        private Color colorLong = Color.FromArgb(255, 128, 128);
        private Color colorDelta = Color.FromArgb(196, 196, 196);

        private Color colorGray = Color.FromArgb(128, 128, 128, 128);
        private Color colorBaseBlue = Color.FromArgb(107, 144, 148);

        private Font fontSmall = new System.Drawing.Font("Calibri Bold Caps", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private Font fontMedium = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private Font fontLarge = new System.Drawing.Font("Calibri Bold Caps", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

        public override void DrawPredictor (System.Drawing.Graphics g, long timePeriod, System.Drawing.RectangleF rect) {

            #region Get And Verify Result Data
            if (results == null || results.Count == 0) {
                Console.WriteLine("RESULTS = NULL or EMPTY");
                DrawNoData(g, rect);
                return;
            }

            long origTimePeriod = timePeriod;

            long endTime = results.Last().timestamp;
            long startTime = endTime - timePeriod;

            int startIndex = 0;
            for (int i = 0; i < results.Count; i++) {
                if (results[i].timestamp > startTime) break;
                startIndex = i;
            }
            if (startIndex == results.Count - 1) {
                DrawNoData(g, rect);
                return;
            }
            startTime = results[startIndex].timestamp;
            timePeriod = endTime - startTime;
            #endregion

            float sizeY = rect.Height - 10;
            float sizeX = rect.Width - 10;

            float gridSize = sizeY / 6;
            float rangeYSize = gridSize * 4;

            float offsetY = rect.Y + 5 + gridSize;
            float offsetX = rect.X + 5;

            Utility.DrawingHelper.DrawGrid(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY), (int)(timePeriod / 60), 6);

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            
            #region Prepare Data
            for (int i = startIndex; i < results.Count; i++) {

                ResultSet.Variable var;
                if(results[i].variables.TryGetValue("volumeShort", out var)) {
                    if(var.value > maxValue) maxValue = var.value;
                    if(var.value < minValue) minValue = var.value;
                }
                if(results[i].variables.TryGetValue("volumeLong", out var)) {
                    if(var.value > maxValue) maxValue = var.value;
                    if(var.value < minValue) minValue = var.value;
                }
			}
            #endregion

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            PointF lastPointShort = new PointF(-5, 0);
            PointF lastPointLong = new PointF(-5, 0);
            PointF lastPointDelta = new PointF(-5, 0);

            double minMaxRange = maxValue - minValue;

            Utility.DrawingHelper.DrawLine(g, new PointF(offsetX, rect.Y + 5 + (sizeY * 0.5f)), new PointF(offsetX + sizeX, rect.Y + 5 + (sizeY * 0.5f)), colorBaseBlue, 2);

            for (int i = startIndex; i < results.Count; i++) {

                #region Find PosX
                float posX = (results[i].timestamp - startTime) / (float)timePeriod;

                if (i == 0) posX = offsetX;
                else if (i + 1 == results.Count) posX = offsetX + sizeX;
                else posX = (posX * sizeX) + offsetX;

                if (float.IsInfinity(posX)) {
                    Console.WriteLine("ERROR - " + timePeriod + " - " + (float)timePeriod);
                }
                #endregion

                #region Draw Short
                ResultSet.Variable var;
                if (results[i].variables.TryGetValue("volumeShort", out var)) {

                    float posY = (float)((var.value - minValue) / minMaxRange);
                    posY = 1 - posY;
                    posY = (posY * rangeYSize) + offsetY;

                    PointF point = new PointF(posX, posY);
                    if (lastPointShort.X < 0) lastPointShort = point;

                    int p1Alpha = GetLineAlpha(lastPointShort.Y, rect.Y + 5, rect.Y + 5 + sizeY);
                    int p2Alpha = GetLineAlpha(posY, rect.Y + 5, rect.Y + 5 + sizeY);

                    if (posX > offsetX) Utility.DrawingHelper.DrawGradientLine(g, lastPointShort, point,
                        Color.FromArgb(p1Alpha, colorShort), Color.FromArgb(p2Alpha, colorShort), 2);

                    lastPointShort = point;
                }
                #endregion

                #region Draw Long
                if (results[i].variables.TryGetValue("volumeLong", out var)) {

                    float posY = (float)((var.value - minValue) / minMaxRange);
                    posY = 1 - posY;
                    posY = (posY * rangeYSize) + offsetY;

                    PointF point = new PointF(posX, posY);
                    if (lastPointLong.X < 0) lastPointLong = point;

                    int p1Alpha = GetLineAlpha(lastPointLong.Y, rect.Y + 5, rect.Y + 5 + sizeY);
                    int p2Alpha = GetLineAlpha(posY, rect.Y + 5, rect.Y + 5 + sizeY);

                    if (posX > offsetX) Utility.DrawingHelper.DrawGradientLine(g, lastPointLong, point, 
                        Color.FromArgb(p1Alpha, colorLong), Color.FromArgb(p2Alpha, colorLong), 2);

                    lastPointLong = point;
                }
                #endregion

            }

            Utility.DrawingHelper.DrawBorders(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY));

            #region Draw Text Labels

            int minPeriod = (int)origTimePeriod / 60;
            Utility.DrawingHelper.DrawShadow(g, "Volume", fontLarge, colorBaseBlue, offsetX + 10, rect.Y + 10);
            Utility.DrawingHelper.DrawShadow(g, "Graph: " + minPeriod + " Minutes", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 2));
            Utility.DrawingHelper.DrawShadow(g, "Period: " + PeriodInterval + " Seconds", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 1));

            float width = 0;

            string settingsString = "Setup: " + Settings[0] + " / " + Settings[1];
            width = g.MeasureString(settingsString, fontMedium).Width;
            Utility.DrawingHelper.DrawShadow(g, settingsString, fontMedium, colorBaseBlue, offsetX + sizeX - width - 10, rect.Y - 5 + sizeY - (fontMedium.Size + 7));

            string pairString = pair.BaseCurrency + " / " + pair.QuoteCurrency;
            width = g.MeasureString(pairString, fontLarge).Width;
            Utility.DrawingHelper.DrawShadow(g, pairString, fontLarge, colorBaseBlue, offsetX + (sizeX / 2) - (width / 2), rect.Y + 10);
            #endregion

            #region Draw Legend
            for (int i = 0; i < 2; i++) {

                string currText;
                Color currColor;

                switch (i) {
                    case 0:
                        currText = "Short SMMA:";
                        currColor = colorShort;
                        break;
                    case 1:
                        currText = "Long SMMA:";
                        currColor = colorLong;
                        break;
                    default:
                        currText = "ERR";
                        currColor = Color.White;
                        break;
                }

                width = g.MeasureString(currText, fontSmall).Width;
                float legendY = rect.Y + fontLarge.Height + 10 + (fontSmall.Height * i);

                Utility.DrawingHelper.DrawShadow(g, currText, fontSmall, colorBaseBlue, offsetX + 10, legendY);
                Utility.DrawingHelper.DrawShadow(g, new RectangleF(offsetX + 10 + width, legendY + 5, fontSmall.Size, fontSmall.Size), currColor);
            }
            #endregion

            g.SmoothingMode = oldSmoothingMode;
        }

        private static int GetLineAlpha (float posY, float min, float max) {
            max -= min;
            posY = (0.5f - Math.Abs((posY - min) / max - 0.5f)) * 3;
            if (posY < 0) posY = 0;
            if (posY > 1) posY = 1;

            posY *= posY;
            return (int)(posY * 255);
        }

    }
}

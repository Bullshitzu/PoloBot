using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class MACD : Predictor {

        public int[] Settings; // = { 150, 900, 60 };

        public MACD (CurrencyPair pair)
            : base(pair) {
            Settings = new int[] { 1500, 1800, 60 }; // 5 min, 10 min
        }
        public MACD (CurrencyPair pair, int shortEma, int longEma)
            : base(pair) {
                Settings = new int[] { shortEma, longEma, 60 };
        }
        public override void SignResult (ResultSet rs) {
            rs.signature = "M.A.C.D.";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double emaShort = GetEMA(tickers, Settings[0]);
            double emaLong = GetEMA(tickers, Settings[1]);

            double macd = emaShort - emaLong;
            double signal = macd;

            if (results != null && results.Count > Settings[2]) {
                List<double> macdValues = new List<double>();
                for (int i = 0; i < Settings[2]; i++) {
                    int index = results.Count - 1 - i;
                    if (index < 0) break;
                    ResultSet.Variable rsTemp;
                    if (results[index].variables.TryGetValue("macd", out rsTemp)) {
                        macdValues.Add(rsTemp.value);
                    }
                }
                macdValues.Reverse();
                signal = Analysis.MovingAverage.ExponentialMovingAverage(macdValues.ToArray());
            }

            double macdHist = macd - signal;
            double macdHistAdj = (macdHist / tickers.Last().MarketData.PriceLast) * 100;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("emaShort", new ResultSet.Variable("EMA (" + Settings[0] + ")", emaShort, 8));
            rs.variables.Add("emaLong", new ResultSet.Variable("EMA (" + Settings[1] + ")", emaLong, 8));
            rs.variables.Add("macd", new ResultSet.Variable("MACD", macd, 8));
            rs.variables.Add("signal", new ResultSet.Variable("Signal (" + Settings[2] + ")", signal, 8));
            rs.variables.Add("macdHistogram", new ResultSet.Variable("Histogram", macdHist, 8));
            rs.variables.Add("macdHistogramAdjusted", new ResultSet.Variable("Histogram (Adjusted)", macdHistAdj, 8));
            
            if (results.Count == 0) SaveResult(rs);
            else SaveResult(rs);

        }

        private double GetEMA (TickerChangedEventArgs[] tickers, int time) {
            long startTime = tickers.Last().Timestamp - time;

            List<double> prices = new List<double>();
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            prices.Reverse();
            return Analysis.MovingAverage.ExponentialMovingAverage(prices.ToArray());
        }
        private double GetSMA (TickerChangedEventArgs[] tickers, int time) {
            long startTime = tickers.Last().Timestamp - time;

            List<double> prices = new List<double>();
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                prices.Add(tickers[i].MarketData.PriceLast);
            }

            prices.Reverse();
            return Analysis.MovingAverage.SimpleMovingAverage(prices.ToArray());
        }

        // -------------------
        // Drawing Variables
        // -------------------

        private Color colorMACD = Color.FromArgb(128, 128, 255);
        private Color colorSignal = Color.FromArgb(255, 128, 128);
        
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

            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.HighQuality;

            float sizeY = rect.Height - 10;
            float sizeX = rect.Width - 10;

            float gridSize = sizeY / 6;
            float rangeYSize = gridSize * 4;

            float offsetY = rect.Y + 5 + gridSize;
            float offsetX = rect.X + 5;

            Utility.DrawingHelper.DrawGrid(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY), (int)(timePeriod / 60), 6);

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            ResultSet.Variable tempVar;

            #region Prepare Data
            for (int i = startIndex; i < results.Count; i++) {

                double macdValue = 0;
                double signalValue = 0;

                if (results[i].variables.TryGetValue("macd", out tempVar)) {
                    macdValue = tempVar.value;
                    if (macdValue < minValue) minValue = macdValue;
                    if (macdValue > maxValue) maxValue = macdValue;
                }
                if (results[i].variables.TryGetValue("signal", out tempVar)) {
                    signalValue = tempVar.value;
                    if (signalValue < minValue) minValue = signalValue;
                    if (signalValue > maxValue) maxValue = signalValue;
                }
            }
            #endregion

            double minMaxRange = maxValue - minValue;

            PointF lastPointMacd = new PointF(-5, 0);
            PointF lastPointSignal = new PointF(-5, 0);

            for (int i = startIndex; i < results.Count; i++) {
                
                #region Find PosX
                float posX = (results[i].timestamp - startTime) / (float)timePeriod;
                posX = (posX * sizeX) + offsetX;
                if (i == 0) posX = rect.X + 5;
                else if (i + 1 == results.Count) posX = offsetX + sizeX;

                if (float.IsInfinity(posX)) {
                    Console.WriteLine("ERROR - " + timePeriod + " - " + (float)timePeriod);
                }
                #endregion

                #region Draw MACD Line
                if (results[i].variables.TryGetValue("macd", out tempVar)) {

                    float posYMacd = (float)((tempVar.value - minValue) / minMaxRange);
                    posYMacd = 1 - posYMacd; // must be inverted because drawing coordinates are inverted too
                    posYMacd = (posYMacd * rangeYSize) + offsetY;

                    PointF pointMacd = new PointF(posX, posYMacd);
                    if (lastPointMacd.X < 0) lastPointMacd = pointMacd;

                    if (posX > offsetX) Utility.DrawingHelper.DrawLine(g, lastPointMacd, pointMacd, colorMACD, 2);
                    lastPointMacd = pointMacd;
                }
                #endregion

                #region Draw Signal Line
                if (results[i].variables.TryGetValue("signal", out tempVar)) {

                    float posYSignal = (float)((tempVar.value - minValue) / minMaxRange);
                    posYSignal = 1 - posYSignal; // must be inverted because drawing coordinates are inverted too
                    posYSignal = (posYSignal * rangeYSize) + offsetY;

                    PointF pointSignal = new PointF(posX, posYSignal);
                    if (lastPointSignal.X < 0) lastPointSignal = pointSignal;

                    if (posX > offsetX) Utility.DrawingHelper.DrawLine(g, lastPointSignal, pointSignal, colorSignal, 2);
                    lastPointSignal = pointSignal;
                }
                #endregion
            }

            Utility.DrawingHelper.DrawBorders(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY));

            #region Draw Text Labels
            int minPeriod = (int)origTimePeriod / 60;
            Utility.DrawingHelper.DrawShadow(g, "M.A.C.D.", fontLarge, colorBaseBlue, offsetX + 10, rect.Y + 10);
            Utility.DrawingHelper.DrawShadow(g, "Graph: " + minPeriod + " Minutes", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 2));
            Utility.DrawingHelper.DrawShadow(g, "Period: 60 Seconds", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 1));

            double spread = ((maxValue - minValue) / maxValue);
            string spreadString = "Scale: " + spread.ToString("F3") + "%";
            float width = g.MeasureString(spreadString, fontMedium).Width;
            Utility.DrawingHelper.DrawShadow(g, spreadString, fontMedium, colorBaseBlue, offsetX + sizeX - width - 10, rect.Y + 10);

            string settingsString = "Setup: " + Settings[0] + " / " + Settings[1] + " / " + Settings[2];
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
                        currText = "MACD:";
                        currColor = colorMACD;
                        break;
                    case 1:
                        currText = "Signal:";
                        currColor = colorSignal;
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

    }
}

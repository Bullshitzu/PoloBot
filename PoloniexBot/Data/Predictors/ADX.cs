using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class ADX : Predictor {

        const int Period = 60;

        public ADX (CurrencyPair pair) : base(pair) { }
        public override void SignResult (ResultSet rs) {
            rs.signature = "A.D.X.";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double upMove = GetUpMove(tickers);
            double downMove = GetDownMove(tickers);

            double dmPos = 0;
            double dmNeg = 0;

            if (upMove < 0 && downMove < 0) dmPos = dmNeg = 0;
            else if (upMove > downMove) {
                dmPos = upMove;
                dmNeg = 0;
            }
            else {
                dmPos = 0;
                dmNeg = downMove;
            }

            double[] atrVars = GetATRVars(tickers);
            double atr = Analysis.Other.AverageTrueRange(atrVars[0], atrVars[1], atrVars[2]);

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            SaveResult(rs); 
            // note: it needs to be saved immediately since it's used by EMA's
            // (doesn't matter for adding vars, it's a reference...)

            rs.variables.Add("dmPos", new ResultSet.Variable("+DM", dmPos, 8));
            rs.variables.Add("dmNeg", new ResultSet.Variable("-DM", dmNeg, 8));
            rs.variables.Add("atr", new ResultSet.Variable("A.T.R.", atr, 8));

            // now +dm14, -dm14, tr14

            double diPos = 0;
            double diNeg = 0;

            if (results.Count > 0) {
                long startTime = tickers.Last().Timestamp - (Period * 14);
                // note: wilders recommends 14 periods but i like round numbers

                List<double> dmPosVars = new List<double>();
                List<double> dmNegVars = new List<double>();
                List<double> atr14Vars = new List<double>();

                for (int i = results.Count - 1; i >= 0; i--) {
                    if (results[i].timestamp < startTime) break;

                    ResultSet.Variable tempVar;
                    if (results[i].variables.TryGetValue("dmPos", out tempVar)) dmPosVars.Add(tempVar.value);
                    if (results[i].variables.TryGetValue("dmNeg", out tempVar)) dmNegVars.Add(tempVar.value);
                    if (results[i].variables.TryGetValue("atr", out tempVar)) atr14Vars.Add(tempVar.value);
                }

                atr14Vars.Reverse();

                double atr14 = (Analysis.MovingAverage.ExponentialMovingAverageWilders(atr14Vars.ToArray()));

                dmPosVars.Reverse();
                dmNegVars.Reverse();

                diPos = Analysis.MovingAverage.ExponentialMovingAverageWilders(dmPosVars.ToArray()) / atr14;
                diNeg = Analysis.MovingAverage.ExponentialMovingAverageWilders(dmNegVars.ToArray()) / atr14;
            }

            // -----------------

            double dx = Math.Abs(diPos - diNeg) / (diPos + diNeg);

            rs.variables.Add("diPos", new ResultSet.Variable("+DI", diPos, 8));
            rs.variables.Add("diNeg", new ResultSet.Variable("-DI", diNeg, 8));
            rs.variables.Add("dx", new ResultSet.Variable("DX", dx, 8));

            // -----------------

            double adx = 100;

            if (results.Count > 0) {
                long startTime = tickers.Last().Timestamp - Period;

                List<double> DXVars = new List<double>();

                for (int i = results.Count - 1; i >= 0; i--) {
                    if (results[i].timestamp < startTime) break;

                    ResultSet.Variable tempVar;
                    if (results[i].variables.TryGetValue("dx", out tempVar)) DXVars.Add(tempVar.value);
                }

                DXVars.Reverse();
                adx = 100 * Analysis.MovingAverage.ExponentialMovingAverageWilders(DXVars.ToArray());
            }

            rs.variables.Add("adx", new ResultSet.Variable("A.D.X.", adx, 8));
        }

        private double GetUpMove (TickerChangedEventArgs[] tickers) {
            
            long startTime1 = tickers.Last().Timestamp - Period;
            long startTime2 = startTime1 - Period;

            double todayHigh = tickers.Last().MarketData.PriceLast;
            double yestHigh = tickers.Last().MarketData.PriceLast;
            
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime2) break;
                if (tickers[i].Timestamp < startTime1) {
                    if (tickers[i].MarketData.PriceLast > yestHigh) yestHigh = tickers[i].MarketData.PriceLast;
                }
                else {
                    if (tickers[i].MarketData.PriceLast > todayHigh) todayHigh = tickers[i].MarketData.PriceLast;
                }
            }

            return todayHigh - yestHigh;
        }
        private double GetDownMove (TickerChangedEventArgs[] tickers) {

            long startTime1 = tickers.Last().Timestamp - Period;
            long startTime2 = startTime1 - Period;

            double todayLow = tickers.Last().MarketData.PriceLast;
            double yestLow = tickers.Last().MarketData.PriceLast;
            
            for (int i = tickers.Length - 1; i >= 0; i--) {    
                if (tickers[i].Timestamp < startTime2) break;
                if (tickers[i].Timestamp < startTime1) {
                    if (tickers[i].MarketData.PriceLast < yestLow) yestLow = tickers[i].MarketData.PriceLast;
                }
                else {
                    if (tickers[i].MarketData.PriceLast < todayLow) todayLow = tickers[i].MarketData.PriceLast;
                }
            }

            return yestLow - todayLow;
        }

        private double[] GetATRVars (TickerChangedEventArgs[] tickers) {

            long startTime = tickers.Last().Timestamp - Period;

            double high = tickers.Last().MarketData.PriceLast;
            double low = tickers.Last().MarketData.PriceLast;
            double previousClose = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                previousClose = tickers[i].MarketData.PriceLast;
                if (tickers[i].Timestamp < startTime) break;
                if (tickers[i].MarketData.PriceLast > high) high = tickers[i].MarketData.PriceLast;
                if (tickers[i].MarketData.PriceLast < low) low = tickers[i].MarketData.PriceLast;
            }

            return new double[] { high, low, previousClose };
        }

        // -------------------
        // Drawing Variables
        // -------------------

        private Color colorADX = Color.FromArgb(255, 128, 128);

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

            double minValue = 0;
            double maxValue = 100;

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            PointF lastPoint = new PointF(-5, 0);

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

                #region Draw A.D.X.
                ResultSet.Variable var;
                if (results[i].variables.TryGetValue("adx", out var)) {

                    float posY = (float)((var.value - minValue) / minMaxRange);
                    posY = 1 - posY;
                    posY = (posY * rangeYSize) + offsetY;

                    PointF point = new PointF(posX, posY);
                    if (lastPoint.X < 0) lastPoint = point;

                    if (posX > offsetX) Utility.DrawingHelper.DrawLine(g, lastPoint, point, colorADX, 2);

                    lastPoint = point;
                }
                #endregion

            }

            Utility.DrawingHelper.DrawBorders(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY));


            #region Draw Text Labels

            int minPeriod = (int)origTimePeriod / 60;
            Utility.DrawingHelper.DrawShadow(g, "A.D.X.", fontLarge, colorBaseBlue, offsetX + 10, rect.Y + 10);
            Utility.DrawingHelper.DrawShadow(g, "Graph: " + minPeriod + " Minutes", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 2));
            Utility.DrawingHelper.DrawShadow(g, "Period: " + Period + " Seconds", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 1));

            float width = 0;

            string pairString = pair.BaseCurrency + " / " + pair.QuoteCurrency;
            width = g.MeasureString(pairString, fontLarge).Width;
            Utility.DrawingHelper.DrawShadow(g, pairString, fontLarge, colorBaseBlue, offsetX + (sizeX / 2) - (width / 2), rect.Y + 10);
            #endregion

            #region Draw Legend
            for (int i = 0; i < 1; i++) {

                string currText;
                Color currColor;

                switch (i) {
                    case 0:
                        currText = "A.D.X.:";
                        currColor = colorADX;
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

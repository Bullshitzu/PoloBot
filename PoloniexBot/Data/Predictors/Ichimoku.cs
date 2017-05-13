using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PoloniexBot.Data.Predictors {
    class Ichimoku : Predictor {

        const int Period = 60;
        static int[] Settings = { 3, 10, 15 };
        const int PeriodInterval = 30;

        const double DropScoreFactor = 1.5;

        const int ChikouAvgCount = 5;

        public Ichimoku (CurrencyPair pair) : base(pair) { }
        
        public override void SignResult (ResultSet rs) {
            rs.signature = "Ichimoku K.H.";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            double tenkanSen = GetTenkanSen(tickers);
            double kijunSen = GetKijunSen(tickers);
            double senkouA = GetSenkouA(tenkanSen, kijunSen);
            double senkouB = GetSenkouB(tickers);
            double chikou = GetChikou(tickers);

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("tenkan", new ResultSet.Variable("Tenkan Sen", tenkanSen, 8));
            rs.variables.Add("kijun", new ResultSet.Variable("Kijun Sen", kijunSen, 8));
            rs.variables.Add("senkouA", new ResultSet.Variable("Senkou A", senkouA, 8));
            rs.variables.Add("senkouB", new ResultSet.Variable("Senkou B", senkouB, 8));
            rs.variables.Add("chikou", new ResultSet.Variable("Chikou", chikou, 8));
            rs.variables.Add("price", new ResultSet.Variable("Price", chikou, 8));

            if(results.Count == 0) SaveResult(rs);
            else SaveResult(rs);

            FindSignals();
        }

        private void FindSignals () {
            if (results == null || results.Count < 2) return;

            double tenkanSen = 0;
            double kijunSen = 0;
            double senkouA = 0;
            double senkouB = 0;
            double chikou = 0;
            double price = 0;

            double lastTenkanSen = 0;
            double lastKijunSen = 0;
            double lastSenkouA = 0;
            double lastSenkouB = 0;
            double lastChikou = 0;
            double lastPrice = 0;

            ResultSet[] resArray = results.ToArray();

            // Get all the values

            ResultSet.Variable tempVar;
            if (resArray.Last().variables.TryGetValue("tenkan", out tempVar)) tenkanSen = tempVar.value;
            if (resArray.Last().variables.TryGetValue("kijun", out tempVar)) kijunSen = tempVar.value;
            if (resArray.Last().variables.TryGetValue("senkouA", out tempVar)) senkouA = tempVar.value;
            if (resArray.Last().variables.TryGetValue("senkouB", out tempVar)) senkouB = tempVar.value;
            if (resArray.Last().variables.TryGetValue("chikou", out tempVar)) chikou = tempVar.value;
            if (resArray.Last().variables.TryGetValue("price", out tempVar)) price = tempVar.value;

            if (resArray[resArray.Length - 2].variables.TryGetValue("tenkan", out tempVar)) lastTenkanSen = tempVar.value;
            if (resArray[resArray.Length - 2].variables.TryGetValue("kijun", out tempVar)) lastKijunSen = tempVar.value;
            if (resArray[resArray.Length - 2].variables.TryGetValue("senkouA", out tempVar)) lastSenkouA = tempVar.value;
            if (resArray[resArray.Length - 2].variables.TryGetValue("senkouB", out tempVar)) lastSenkouB = tempVar.value;
            if (resArray[resArray.Length - 2].variables.TryGetValue("chikou", out tempVar)) lastChikou = tempVar.value;
            if (resArray[resArray.Length - 2].variables.TryGetValue("price", out tempVar)) lastPrice = tempVar.value;

            // Calculate Score

            double tkSize = ((tenkanSen / kijunSen) - 1) * 100;
            tkSize *= 2; // note: adjustable variable - relevance of tenkan/kijun distance

            results.Last().variables.Add("tkSize", new ResultSet.Variable("T/K Size", tkSize, 8));

            double tkAvg = (tenkanSen + kijunSen) / 2;
            double senkouAvg = (senkouA + senkouB) / 2;
            double TkSenkouPos = ((tkAvg / senkouAvg) - 1) * 250;

            // factors:
            // tkSize
            // abs(tkSenkouPos)

            double totalScore = tkSize * TkSenkouPos;

            if (TkSenkouPos > 0) {
                if (tkSize < 0) totalScore *= DropScoreFactor;
            }
            else {
                totalScore *= -1;
                if (tkSize > 0) totalScore *= DropScoreFactor;
            }

            results.Last().variables.Add("score", new ResultSet.Variable("Score", totalScore, 8));

        }

        static double GetTenkanSen (TickerChangedEventArgs[] tickers) {

            long startTime = tickers.Last().Timestamp - (Settings[0] * Period);

            double min = tickers.Last().MarketData.PriceLast;
            double max = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                if (tickers[i].MarketData.PriceLast < min) min = tickers[i].MarketData.PriceLast;
                if (tickers[i].MarketData.PriceLast > max) max = tickers[i].MarketData.PriceLast;
            }

            return (min + max) / 2;
        }
        static double GetKijunSen (TickerChangedEventArgs[] tickers) {
            
            long startTime = tickers.Last().Timestamp - (Settings[1] * Period);

            double min = tickers.Last().MarketData.PriceLast;
            double max = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                if (tickers[i].MarketData.PriceLast < min) min = tickers[i].MarketData.PriceLast;
                if (tickers[i].MarketData.PriceLast > max) max = tickers[i].MarketData.PriceLast;
            }

            return (min + max) / 2;
        }
        static double GetSenkouA (double tenkanSen, double kijunSen) {
            return (tenkanSen + kijunSen) / 2;
        }
        static double GetSenkouB (TickerChangedEventArgs[] tickers) {
            
            long startTime = tickers.Last().Timestamp - (Settings[2] * Period);

            double min = tickers.Last().MarketData.PriceLast;
            double max = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                if (tickers[i].MarketData.PriceLast < min) min = tickers[i].MarketData.PriceLast;
                if (tickers[i].MarketData.PriceLast > max) max = tickers[i].MarketData.PriceLast;
            }

            return (min + max) / 2;
        }
        static double GetChikou (TickerChangedEventArgs[] tickers) {

            double avg = 0;
            int avgCount = 0;

            for (int i = tickers.Length - 1; i > 0 && i > tickers.Length - ChikouAvgCount - 1; i--) {
                avg += tickers[i].MarketData.PriceLast;
                avgCount++;
            }

            if (avgCount > 0) avg /= avgCount;
            return avg;
        }

        // -------------------
        // Drawing Variables
        // -------------------

        private Color colorTenkan = Color.FromArgb(128, 128, 255);
        private Color colorKijun = Color.FromArgb(255, 128, 128);
        private Color colorSenkouA = Color.FromArgb(220, 220, 50);
        private Color colorSenkouB = Color.FromArgb(128, 128, 35);
        private Color colorChikou = Color.FromArgb(64, 192, 64);

        private Color colorGray = Color.FromArgb(128, 128, 128, 128);
        private Color colorBaseBlue = Color.FromArgb(107, 144, 148);

        private Font fontSmall = new System.Drawing.Font("Calibri Bold Caps", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private Font fontMedium = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private Font fontLarge = new System.Drawing.Font("Calibri Bold Caps", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

        private class RefDouble {
            public double value;
            public RefDouble (double value) {
                this.value = value;
            }
        }
        private struct DataSet {
            public long timestamp;

            public RefDouble tenkan;
            public RefDouble kijun;
            public RefDouble senkouA;
            public RefDouble senkouB;
            public RefDouble chikou;

            public DataSet (long timestamp) {
                this.timestamp = timestamp;
                tenkan = kijun = senkouA = senkouB = chikou = null;
            }
        }

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
            long senkouStartTime = startTime - (Settings[1] * Period);
            int startIndex = 0;
            int senkouStartIndex = 0;
            for (int i = 0; i < results.Count; i++) {
                if (results[i].timestamp < senkouStartTime) senkouStartIndex = i;
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

            float splitSize = sizeX * 0.8f;

            float gridSize = sizeY / 6;
            float rangeYSize = gridSize * 4;

            float offsetY = rect.Y + 5 + gridSize;
            float offsetX = rect.X + 5;

            Utility.DrawingHelper.DrawGrid(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY), (int)(timePeriod / 60), 6);
            Utility.DrawingHelper.DrawLine(g, new PointF(offsetX, offsetY + (2 * gridSize)), new PointF(offsetX + sizeX, offsetY + (2 * gridSize)), colorBaseBlue, 2);

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            List<DataSet> data = new List<DataSet>();

            #region Prepare Data
            for (int i = senkouStartIndex; i < results.Count; i++) {

                ResultSet.Variable tempVar;
                DataSet ds = new DataSet(results[i].timestamp);

                if (results[i].variables.TryGetValue("tenkan", out tempVar)) ds.tenkan = new RefDouble(tempVar.value);
                if (results[i].variables.TryGetValue("kijun", out tempVar)) ds.kijun = new RefDouble(tempVar.value);
                if (results[i].variables.TryGetValue("senkouA", out tempVar)) ds.senkouA = new RefDouble(tempVar.value);
                if (results[i].variables.TryGetValue("senkouB", out tempVar)) ds.senkouB = new RefDouble(tempVar.value);
                if (results[i].variables.TryGetValue("chikou", out tempVar)) ds.chikou = new RefDouble(tempVar.value);

                if (ds.tenkan != null) {
                    if (ds.tenkan.value > maxValue) maxValue = ds.tenkan.value;
                    if (ds.tenkan.value < minValue) minValue = ds.tenkan.value;
                }
                if (ds.kijun != null) {
                    if (ds.kijun.value > maxValue) maxValue = ds.kijun.value;
                    if (ds.kijun.value < minValue) minValue = ds.kijun.value;
                }
                if (ds.senkouA != null) {
                    if (ds.senkouA.value > maxValue) maxValue = ds.senkouA.value;
                    if (ds.senkouA.value < minValue) minValue = ds.senkouA.value;
                }
                if (ds.senkouB != null) {
                    if (ds.senkouB.value > maxValue) maxValue = ds.senkouB.value;
                    if (ds.senkouB.value < minValue) minValue = ds.senkouB.value;
                }
                if (ds.chikou != null) {
                    if (ds.chikou.value > maxValue) maxValue = ds.chikou.value;
                    if (ds.chikou.value < minValue) minValue = ds.chikou.value;
                }

                data.Add(ds);
            }
            #endregion

            double minMaxRange = maxValue - minValue;

            PointF lastPointTenkan = new PointF(-5, 0);
            PointF lastPointKijun = new PointF(-5, 0);
            PointF lastPointSenkouA = new PointF(-5, 0);
            PointF lastPointSenkouB = new PointF(-5, 0);
            PointF lastPointChikou = new PointF(-5, 0);

            for (int i = 0; i < data.Count; i++) {

                #region Find PosX
                float posX = (data[i].timestamp - startTime) / (float)timePeriod;
                float posXSenkou = posX;
                float posXChikou = posX;
                posX = (posX * splitSize) + offsetX;
                if (i == 0) posX = rect.X + 5;
                else if (i + 1 == data.Count) posX = offsetX + splitSize;
                
                posXSenkou += 0.2f;
                posXSenkou = (posXSenkou * splitSize) + offsetX;
                if (posXSenkou < offsetX) posXSenkou = offsetX;

                posXChikou -= 0.2f;
                posXChikou = (posXChikou * splitSize) + offsetX;
                if (posXChikou < offsetX || i == 0) posXChikou = offsetX;

                if (float.IsInfinity(posX) || float.IsInfinity(posXSenkou) || float.IsInfinity(posXChikou)) {
                    Console.WriteLine("ERROR - " + timePeriod + " - " + (float)timePeriod);
                }
                #endregion

                #region Draw Chikou
                if (data[i].chikou != null) {
                    float posYChikou = (float)((data[i].chikou.value - minValue) / minMaxRange);
                    posYChikou = 1 - posYChikou; // must be inverted because drawing coordinates are inverted too
                    posYChikou = (posYChikou * rangeYSize) + offsetY;

                    PointF pointChikou = new PointF(posXChikou, posYChikou);
                    if (lastPointChikou.X < 0) lastPointChikou = pointChikou;

                    if (posXChikou > offsetX) Utility.DrawingHelper.DrawLine(g, lastPointChikou, pointChikou, Color.FromArgb(192, colorChikou), 2);
                    lastPointChikou = pointChikou;
                }
                #endregion

                #region Draw Senkou & Kumo
                if (data[i].senkouA != null && data[i].senkouB != null) {
                    float posYSenkouA = (float)((data[i].senkouA.value - minValue) / minMaxRange);
                    posYSenkouA = 1 - posYSenkouA; // must be inverted because drawing coordinates are inverted too
                    posYSenkouA = (posYSenkouA * rangeYSize) + offsetY;

                    float posYSenkouB = (float)((data[i].senkouB.value - minValue) / minMaxRange);
                    posYSenkouB = 1 - posYSenkouB; // must be inverted because drawing coordinates are inverted too
                    posYSenkouB = (posYSenkouB * rangeYSize) + offsetY;

                    PointF pointSenkouA = new PointF(posXSenkou, posYSenkouA);
                    PointF pointSenkouB = new PointF(posXSenkou, posYSenkouB);

                    if (lastPointSenkouA.X < 0) lastPointSenkouA = pointSenkouA;
                    if (lastPointSenkouB.X < 0) lastPointSenkouB = pointSenkouB;

                    if (posXSenkou > offsetX) {
                        Utility.DrawingHelper.DrawGradientLine(g, pointSenkouA, pointSenkouB,
                            Color.FromArgb(128, colorSenkouA), Color.FromArgb(128, colorSenkouB), 1);
                        Utility.DrawingHelper.DrawLine(g, lastPointSenkouA, pointSenkouA, Color.FromArgb(192, colorSenkouA), 1);
                        Utility.DrawingHelper.DrawLine(g, lastPointSenkouB, pointSenkouB, Color.FromArgb(192, colorSenkouB), 1);
                    }

                    lastPointSenkouA = pointSenkouA;
                    lastPointSenkouB = pointSenkouB;
                }
                #endregion

                #region Draw Tenkan Sen
                if (data[i].tenkan != null) {
                    float posYTenkan = (float)((data[i].tenkan.value - minValue) / minMaxRange);
                    posYTenkan = 1 - posYTenkan; // must be inverted because drawing coordinates are inverted too
                    posYTenkan = (posYTenkan * rangeYSize) + offsetY;

                    PointF pointTenkan = new PointF(posX, posYTenkan);
                    if (lastPointTenkan.X < 0) lastPointTenkan = pointTenkan;

                    if (posX > offsetX) Utility.DrawingHelper.DrawLine(g, lastPointTenkan, pointTenkan, colorTenkan, 2);
                    lastPointTenkan = pointTenkan;
                }
                #endregion

                #region Draw Kijun Sen
                if (data[i].kijun != null) {
                    float posYKijun = (float)((data[i].kijun.value - minValue) / minMaxRange);
                    posYKijun = 1 - posYKijun; // must be inverted because drawing coordinates are inverted too
                    posYKijun = (posYKijun * rangeYSize) + offsetY;

                    PointF pointKijun = new PointF(posX, posYKijun);
                    if (lastPointTenkan.X < 0) lastPointTenkan = pointKijun;

                    if(posX > offsetX) Utility.DrawingHelper.DrawLine(g, lastPointKijun, pointKijun, colorKijun, 2);
                    lastPointKijun = pointKijun;
                }
                #endregion

            }

            Utility.DrawingHelper.DrawLine(g, new PointF(offsetX + splitSize, rect.Y + 5), new PointF(offsetX + splitSize, rect.Y + 5 + sizeY), colorBaseBlue, 2);

            #region Draw Dots
            Utility.DrawingHelper.DrawCircle(g, lastPointChikou.X, lastPointChikou.Y, 4, colorChikou);
            Utility.DrawingHelper.DrawDashedLine(g, lastPointChikou, new PointF(offsetX + splitSize, lastPointChikou.Y), colorChikou, 2);
            Utility.DrawingHelper.DrawCircle(g, offsetX + splitSize, lastPointChikou.Y, 4, colorChikou);

            Utility.DrawingHelper.DrawCircle(g, lastPointTenkan.X, lastPointTenkan.Y, 4, colorTenkan);
            Utility.DrawingHelper.DrawCircle(g, lastPointKijun.X, lastPointKijun.Y, 4, colorKijun);
            #endregion

            Utility.DrawingHelper.DrawBorders(g, new PointF(offsetX, rect.Y + 5), new PointF(offsetX + sizeX, rect.Y + 5 + sizeY));
            
            #region Draw Text Labels
            int minPeriod = (int)origTimePeriod / 60;
            Utility.DrawingHelper.DrawShadow(g, "Ichimoku K.H.", fontLarge, colorBaseBlue, offsetX + 10, rect.Y + 10);
            Utility.DrawingHelper.DrawShadow(g, "Graph: " + minPeriod + " Minutes", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 2));
            Utility.DrawingHelper.DrawShadow(g, "Period: 60 Seconds", fontMedium, colorBaseBlue, offsetX + 10, rect.Y - 5 + sizeY - ((fontMedium.Size + 7) * 1));

            double spread = ((maxValue - minValue) / maxValue) * 100;
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
            for (int i = 0; i < 5; i++) {

                string currText;
                Color currColor;

                switch (i) {
                    case 0:
                        currText = "Tenkan Sen:";
                        currColor = colorTenkan;
                        break;
                    case 1:
                        currText = "Kijun Sen:";
                        currColor = colorKijun;
                        break;
                    case 2:
                        currText = "Senkou A:";
                        currColor = colorSenkouA;
                        break;
                    case 3:
                        currText = "Senkou B:";
                        currColor = colorSenkouB;
                        break;
                    case 4:
                        currText = "Chikou:";
                        currColor = colorChikou;
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

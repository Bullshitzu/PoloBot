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

    }
}

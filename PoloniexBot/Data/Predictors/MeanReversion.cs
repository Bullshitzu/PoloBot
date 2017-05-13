using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PoloniexBot.Data.Predictors {
    class MeanReversion : Predictor {

        public MeanReversion (CurrencyPair pair) : base(pair) { }
        public override void SignResult (ResultSet rs) {
            rs.signature = "Mean Rev.";
        }

        // -------------------
        // Setup Vars
        // -------------------

        private const long MeanTimePeriod = 3600; // 1 hour

        // -------------------

        public void Recalculate (TickerChangedEventArgs[] tickers) {
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            double meanPrice = CalculateMeanPrice(tickers, MeanTimePeriod);
            double currPrice = tickers.Last().MarketData.PriceLast;

            // 10% under = +1
            // 10% over = -1

            double ratio = ((meanPrice - currPrice) / meanPrice) * 50;
            if (double.IsNaN(ratio) || double.IsInfinity(ratio)) ratio = 0;

            rs.variables.Add("price", new ResultSet.Variable("Price", currPrice, 8));
            rs.variables.Add("score", new ResultSet.Variable("Score", ratio, 4));
            SaveResult(rs);
            
        }

        private double CalculateMeanPrice (TickerChangedEventArgs[] tickers, long timePeriod) {

            long currTime = tickers.Last().Timestamp;
            long startTime = currTime - timePeriod;

            int startIndex = tickers.Length - 1;
            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;
                startIndex = i;
            }

            double sum = 0;
            int sumCount = 0;
            for (int i = startIndex; i < tickers.Length; i++) {
                sum += tickers[i].MarketData.PriceLast;
                sumCount++;
            }

            return sum / sumCount;
        }

        // -------------------
        // Drawing Variables
        // -------------------

        private Color colorGray = Color.FromArgb(128, 128, 128, 128);
        private Color colorBaseBlue = Color.FromArgb(107, 144, 148);

        private Font fontSmall = new System.Drawing.Font("Calibri Bold Caps", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private Font fontMedium = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private Font fontLarge = new System.Drawing.Font("Calibri Bold Caps", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

        // -------------------

        public override void DrawPredictor (Graphics g, long timePeriod, RectangleF rect) {
            // todo: this
        }


    }
}

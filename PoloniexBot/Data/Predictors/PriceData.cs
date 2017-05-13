using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PoloniexBot.Data.Predictors {
    class PriceData : Predictor {

        public PriceData (CurrencyPair pair) : base(pair) { } 
        public override void SignResult (ResultSet rs) {
            rs.signature = "Raw Data";
        }

        // -------------------

        public void Recalculate (TickerChangedEventArgs[] tickers) {
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            // todo: other stuff here? (volume, trades/sec...)
            double currPrice = tickers.Last().MarketData.PriceLast;

            rs.variables.Add("price", new ResultSet.Variable("Price", currPrice, 8));
            SaveResult(rs);
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

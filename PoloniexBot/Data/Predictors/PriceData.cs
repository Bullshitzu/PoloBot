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

    }
}

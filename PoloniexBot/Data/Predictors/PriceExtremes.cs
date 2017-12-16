using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class PriceExtremes : Predictor {

        public PriceExtremes (CurrencyPair pair) : base(pair) { }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Price Extremes";
        }

        public double CurrentPrice = 0;
        public double CurrentMinimum = double.MaxValue;
        public double CurrentMaximum = double.MinValue;

        public double PriceRiseOffset = 0;

        public void Update (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            PriceRiseOffset -= 0.1;
            if (PriceRiseOffset < 0) PriceRiseOffset = 0;
            if (PriceRiseOffset > 1) PriceRiseOffset = 1;

            CurrentPrice = tickers.Last().MarketData.PriceLast;
            if (CurrentPrice < CurrentMinimum) CurrentMinimum = CurrentPrice;
            if (CurrentPrice > CurrentMaximum) {
                CurrentMaximum = CurrentPrice;
                PriceRiseOffset = 1;
            }

            rs.variables.Add("price", new ResultSet.Variable("Price", CurrentPrice, 8));
            rs.variables.Add("min", new ResultSet.Variable("Min", CurrentMinimum, 8));
            rs.variables.Add("max", new ResultSet.Variable("Max", CurrentMaximum, 8));

            SaveResult(rs);
        }
    }
}

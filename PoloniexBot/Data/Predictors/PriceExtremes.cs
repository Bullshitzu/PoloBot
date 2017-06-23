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

        static int MinimumTimespan = 3600; // 1 hour
        static int MaximumTimespan = int.MaxValue; // Indefinite

        static double BandSize = 0.95;

        public double CurrentMinimum = double.MaxValue;
        public double CurrentMaximum = double.MinValue;

        public void Update (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            double currPrice = tickers.Last().MarketData.PriceLast;

            if (currPrice < CurrentMinimum) CurrentMinimum = currPrice;
            if (currPrice > CurrentMaximum) {
                CurrentMaximum = currPrice;

                double minTolerated = CurrentMaximum * BandSize;
                if (CurrentMinimum < minTolerated) CurrentMinimum = minTolerated;
            }
            
            rs.variables.Add("min", new ResultSet.Variable("Min", CurrentMinimum, 8));
            rs.variables.Add("max", new ResultSet.Variable("Max", CurrentMaximum, 8));

            SaveResult(rs);
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);

            double min = FindMinimum(tickers, MinimumTimespan);
            double max = FindMaxiumum(tickers, MaximumTimespan);

            CurrentMinimum = min;
            CurrentMaximum = max;

            rs.variables.Add("min", new ResultSet.Variable("Min", min, 8));
            rs.variables.Add("max", new ResultSet.Variable("Max", max, 8));

            SaveResult(rs);
        }

        public double FindMinimum (TickerChangedEventArgs[] tickers, int timeDelta) {

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - timeDelta;

            double min = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                double currPrice = tickers[i].MarketData.PriceLast;
                if (currPrice < min) min = currPrice;
            }

            return min;
        }
        public double FindMaxiumum (TickerChangedEventArgs[] tickers, int timeDelta) {

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - timeDelta;

            double max = tickers.Last().MarketData.PriceLast;

            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                double currPrice = tickers[i].MarketData.PriceLast;
                if (currPrice > max) max = currPrice;
            }

            return max;
        }
    }
}

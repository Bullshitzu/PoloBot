using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class BollingerBands : Predictor {

        private const int Timespan = 300; // seconds

        public BollingerBands (CurrencyPair pair) : base(pair) { }
        public override void SignResult (ResultSet rs) {
            rs.signature = "Bollinger Bands";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            // price SMA over timespan
            double sma = GetSMA(tickers);

            // standard deviation
            double stDev = GetStDev(tickers, sma);
            double volatility = ((stDev * 2) / sma) * 100;

            // upper and lower bands
            double upperBand = sma + (stDev * 2);
            double lowerBand = sma - (stDev * 2);

            // save results
            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("sma", new ResultSet.Variable("SMA", sma, 8));
            rs.variables.Add("upperBand", new ResultSet.Variable("Upper Band", upperBand, 8));
            rs.variables.Add("lowerBand", new ResultSet.Variable("Lower Band", lowerBand, 8));
            rs.variables.Add("volatility", new ResultSet.Variable("Volatility", volatility, 8));

            SaveResult(rs);
        }

        private double GetSMA (TickerChangedEventArgs[] tickers) {

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - Timespan;

            double sum = 0;
            int sumCount = 0;
            for (int i = tickers.Length-1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                sum += tickers[i].MarketData.PriceLast;
                sumCount++;
            }

            return sum / sumCount;
        }

        private double GetStDev (TickerChangedEventArgs[] tickers, double SMA) {
            
            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - Timespan;

            double sum = 0;
            int sumCount = 0;
            for (int i = tickers.Length - 1; i >= 0; i--) {
                if (tickers[i].Timestamp < startTime) break;

                sum += Math.Abs((SMA - tickers[i].MarketData.PriceLast));
                sumCount++;
            }

            return sum / sumCount;
        }
    }
}

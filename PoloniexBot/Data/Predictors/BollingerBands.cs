using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class BollingerBands : Predictor {

        private const int BandDeltaTime = 3600; // seconds

        private int localTimespan = 300;

        public BollingerBands (CurrencyPair pair, int timespan = 300) : base(pair) {
            localTimespan = timespan;
        }
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

            // upper and lower bands
            double upperBand = sma + (stDev * 2);
            double lowerBand = sma - (stDev * 2);

            double bandSize = upperBand - lowerBand;
            double bandSizeSMA = bandSize;

            // band size delta
            if (results != null && results.Count > 10) {
                long breakTimestamp = tickers.Last().Timestamp - BandDeltaTime;

                double sum = 0;
                int sumCount = 0;

                for (int i = results.Count - 1; i > 0; i--) {
                    if (results[i].timestamp < breakTimestamp) break;
                    
                    ResultSet.Variable rsTemp;
                    if (results[i].variables.TryGetValue("bandSize", out rsTemp)) {
                        sum += rsTemp.value;
                        sumCount++;
                    }
                }

                bandSizeSMA = sum /= sumCount;
            }

            double bandSizeDelta = ((bandSize - bandSizeSMA) / bandSizeSMA) * 100;

            // save results
            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("sma", new ResultSet.Variable("SMA", sma, 8));
            rs.variables.Add("upperBand", new ResultSet.Variable("Upper Band", upperBand, 8));
            rs.variables.Add("lowerBand", new ResultSet.Variable("Lower Band", lowerBand, 8));
            rs.variables.Add("bandSize", new ResultSet.Variable("Band Size", bandSize, 8));
            rs.variables.Add("bandSizeDelta", new ResultSet.Variable("Band Size Delta", bandSizeDelta, 8));

            SaveResult(rs);
        }

        private double GetSMA (TickerChangedEventArgs[] tickers) {

            long endTime = tickers.Last().Timestamp;
            long startTime = endTime - localTimespan;

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
            long startTime = endTime - localTimespan;

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

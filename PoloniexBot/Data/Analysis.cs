using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data {
    public static class Analysis {

        public static class MovingAverage {

            public static double SimpleMovingAverage (double[] values) {
                if (values == null || values.Length == 0) return 0;
                double sum = 0;
                for (int i = 0; i < values.Length; i++) {
                    sum += values[i];
                }
                return sum / values.Length;
            }
            public static TickerChangedEventArgs[] SimpleMovingAverage (TickerChangedEventArgs[] tickers) {

                List<TickerChangedEventArgs> smoothedTickers = new List<TickerChangedEventArgs>();

                List<double> prices = new List<double>();
                for (int i = 0; i < tickers.Length; i++) {
                    prices.Add(tickers[i].MarketData.PriceLast);
                    while (prices.Count > 20) prices.RemoveAt(0);

                    double currPrice = Analysis.MovingAverage.SimpleMovingAverage(prices.ToArray());
                    smoothedTickers.Add(new TickerChangedEventArgs(tickers[i], currPrice));
                }

                return smoothedTickers.ToArray();
            }

            public static double SmoothedMovingAverage (double[] values) {
                if (values == null || values.Length == 0) return 0;

                double sum = 0;
                for (int i = 0; i < values.Length; i++) {
                    sum += values[i];
                }
                double smma1 = sum / values.Length;
                return (sum - smma1 + values.Last()) / values.Length;
            }

            public static double ExponentialMovingAverage (double[] values) {
                if (values == null || values.Length == 0) return 0;

                // 20% weight on most recent
                double sum = 0;
                for (int i = 0; i < values.Length; i++) {
                    double mult = ((i + 1) / (double)values.Length) * 0.2 + 0.9;
                    sum += values[i] * mult;
                }

                return sum / values.Length;
            }

            public static double ExponentialMovingAverageWilders (double[] values) {
                // note: for use by ADX

                return SimpleMovingAverage(values);

                // todo: this
                // *Wilder calculated moving average differently, owing to the need for calculating averages quickly by hand. For example:
                // Current +DM14 = 13/14 (Previous +DM14) + 1/14 (Current +DM).
                // The first +DM14 value in the series, was simply the sum of the previous fourteen -DM14 values divided by 14 (SMA).
            }

        }

        public static class Other {

            public static double AverageTrueRange (double high, double low, double previousClose) {

                double atr0 = high - low;
                double atr1 = Math.Abs(high - previousClose);
                double atr2 = Math.Abs(low - previousClose);

                double atr = Math.Max(atr0, atr1);
                atr = Math.Max(atr, atr2);

                return atr;
            }

            public static double RelativeStrenghtIndex (double[] prices) {

                double gain = 0;
                double loss = 0;
                int gainCount = 0;
                int lossCount = 0;

                for (int i = 1; i < prices.Length; i++) {
                    double delta = prices[i] - prices[i - 1];

                    if (delta > 0) {
                        gain += delta;
                        gainCount++;
                    }
                    else if (delta < 0) {
                        loss += delta;
                        lossCount++;
                    }
                }

                gain /= gainCount;
                loss /= -lossCount;

                double rs = 100 - (100 / (1 + (gain / loss)));
                return rs;
            }
        }
    }
}

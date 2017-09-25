using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class FuturePriceExtremes : Predictor {

        public FuturePriceExtremes (CurrencyPair pair) : base(pair) { }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Future Price Extremes";
        }

        // ----------------------------------

        private const long Timeframe = 7200; // 2 hours

        public void Calculate (Data.Precalculation.DataPoint[] dataPoints) {
            if (dataPoints == null) return;

            for (int i = 0; i < dataPoints.Length; i++) {

                double currPrice = dataPoints[i].Result;

                long startTime = dataPoints[i].Timestamp;
                long endTime = startTime + Timeframe;

                double min = currPrice;
                double max = currPrice;

                for (int j = i; j < dataPoints.Length; j++) {
                    if (dataPoints[j].Timestamp > endTime) break;

                    double checkPrice = dataPoints[j].Result;

                    if (checkPrice < min) min = checkPrice;
                    if (checkPrice > max) max = checkPrice;
                }

                min = currPrice - min;
                max = max - currPrice;

                dataPoints[i].Result = ((max - min) / currPrice) * 100;
            }
        }
    }
}

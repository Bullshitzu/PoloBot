using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class PatternMatch : Predictor {

        public PatternMatch (CurrencyPair pair) : base(pair) { }

        public override void SignResult (ResultSet rs) {
            rs.signature = "Pattern Match";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;

            Data.PatternMatching.Pattern p = Data.PatternMatching.Manager.BuildPattern(tickers, tickers.Length - 1, false);
            Data.PatternMatching.Pattern[] matches = Data.PatternMatching.Manager.AnalyzePattern(p, pair);

            double prediction = GetAveragePrediction(matches, 5);

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("result", new ResultSet.Variable("Prediction", prediction, 8));

            SaveResult(rs);

        }

        private double GetAveragePrediction (PatternMatching.Pattern[] patterns, int count) {
            if (patterns == null) return 0;

            double sum = 0;
            int cnt = 0;
            for (int i = 0; i < patterns.Length && i < count; i++) {
                sum += patterns[i].movement.Last();
                cnt++;
            }
            return sum / cnt;
        }

    }
}

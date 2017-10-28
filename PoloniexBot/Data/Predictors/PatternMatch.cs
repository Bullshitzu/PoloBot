using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.Predictors {
    class PatternMatch : Predictor {

        public PatternMatch (CurrencyPair pair, Data.ANN.Network ann) : base(pair) {
            this.ANN = ann;
        }

        private Data.ANN.Network ANN;

        public override void SignResult (ResultSet rs) {
            rs.signature = "Pattern Match";
        }

        public void Recalculate (object dataSet) {
            TickerChangedEventArgs[] tickers = (TickerChangedEventArgs[])dataSet;
            if (tickers == null || tickers.Length == 0) return;
            if (ANN == null) return;

            Data.PatternMatching.Pattern p = Data.PatternMatching.Manager.BuildPattern(tickers, tickers.Length - 1, false);
            Data.PatternMatching.Pattern closestMatch = Data.PatternMatching.Manager.AnalyzePattern(p, pair);

            ANN.SetInputs(closestMatch.movement, true);
            ANN.Recalculate();
            double prediction = ANN.GetOutputs().First();

            ResultSet rs = new ResultSet(tickers.Last().Timestamp);
            rs.variables.Add("result", new ResultSet.Variable("Prediction", prediction, 8));

            SaveResult(rs);

        }

    }
}

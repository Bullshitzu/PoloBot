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

        public void Recalculate (object dataSet, long timestamp) {
            Data.PatternMatching.Pattern p = (Data.PatternMatching.Pattern)dataSet;
            if (p == null) return;

            double buySignal = PatternMatching.Manager.AnalyzePattern(p, Trading.MarketAction.Buy);
            double sellSignal = PatternMatching.Manager.AnalyzePattern(p, Trading.MarketAction.Sell);

            ResultSet rs = new ResultSet(timestamp);
            rs.variables.Add("buySignal", new ResultSet.Variable("Buy Signal", buySignal, 8));
            rs.variables.Add("sellSignal", new ResultSet.Variable("Sell Signal", sellSignal, 8));

            if (results.Count == 0) SaveResult(rs);
            else SaveResult(rs);
        }
    }
}

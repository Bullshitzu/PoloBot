using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Trading.Rules {
    class RuleGlobalDrop : TradeRule {

        private static List<KeyValuePair<CurrencyPair, double>> Pairs;
        private static double GlobalTrend = 0;

        private const double BlockBuyTrigger = -0.5;
        private const double SellTrigger = 3;

        private string VariableName = "";

        public RuleGlobalDrop (string varName) {
            VariableName = varName;
        }

        public void Recalculate (Dictionary<string, double> values, CurrencyPair pair) {
            if (Pairs == null) Pairs = new List<KeyValuePair<CurrencyPair, double>>();

            double priceDelta = 0;
            if (!values.TryGetValue(VariableName, out priceDelta)) throw new VariableNotIncludedException();

            // find pair on the list and update value
            bool found = false;
            for (int i = 0; i < Pairs.Count; i++) {
                if (Pairs[i].Key == pair) {
                    Pairs[i] = new KeyValuePair<CurrencyPair, double>(pair, priceDelta);
                    found = true;
                    break;
                }
            }

            // its not on the list, so add it
            if (!found) Pairs.Add(new KeyValuePair<CurrencyPair, double>(pair, priceDelta));

            // now recalculate global trend
            GlobalTrend = GetGlobalTrend();

            if (GlobalTrend > SellTrigger) {
                currentResult = RuleResult.Sell;
                return;
            }

            if (GlobalTrend < BlockBuyTrigger) {
                currentResult = RuleResult.BlockBuy;
                return;
            }

            currentResult = RuleResult.None;
        }

        public static double GetGlobalTrend () {
            if (Pairs == null) return 0;
            // if (ClientManager.Training) return 0;

            double sum = 0;
            for (int i = 0; i < Pairs.Count; i++) {
                sum += Pairs[i].Value;
            }
            sum /= Pairs.Count;

            return sum;
        }

        public static void ClearUnusedPairs (TPManager[] legalPairs) {
            if (Pairs == null) return;
            if (legalPairs == null) return;

            for (int i = 0; i < Pairs.Count; i++) {

                bool found = false;
                for (int j = 0; j < legalPairs.Length; j++) {
                    if (Pairs[i].Key == legalPairs[j].GetPair()) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    Pairs.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void Recalculate (Dictionary<string, double> values) {
            throw new NotImplementedException();
        }


    }
}

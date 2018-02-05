using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexBot.Trading.Rules;
using PoloniexAPI;

namespace PoloniexBot.Trading.Strategies {
    class ArbitrageStep1 : Strategy {

        public ArbitrageStep1 (CurrencyPair pair) : base(pair) { }

        public override void Reset () {
            base.Reset();
            Setup(true);
        }

        public override void Setup (bool simulate = false) {
            throw new NotImplementedException();
        }

        public override void UpdatePredictors () {
            



        }

        public override void EvaluateTrade () {
            throw new NotImplementedException();
        }
    }
}

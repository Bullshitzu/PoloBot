using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data.TriArbitrage {
    class PairMonitor {

        public CurrencyPair pair1;
        public CurrencyPair pair2;

        public PairMonitor (string quote, string base1, string base2) {
            this.pair1 = new CurrencyPair(base1, quote);
            this.pair2 = new CurrencyPair(base2, quote);
        }

        Trading.Strategies.ArbitrageStep1 step1Strat;
        Trading.Strategies.ArbitrageStep2 step2Strat;
        Trading.Strategies.ArbitrageStep3 step3Strat;

        public void Update () {
            // note: called by strat1 or strat2 after ticker update

            // todo: pull new prices / orders from 3 strats

            // todo: simulate a triangular arbitrage

            // todo: if profitable, execute


        }


    }
}

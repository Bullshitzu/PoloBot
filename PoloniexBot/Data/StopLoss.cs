using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.Data {
    class StopLoss {

        protected CurrencyPair pair;

        public StopLoss (CurrencyPair pair) {
            this.pair = pair;
        }

        private bool enabled = false;
        double highestValue=0;
        const float lossMargin = 0.98f; // 0.5% loss triggers sale

        public void SetPrice (double value) {
            highestValue = value;
            enabled = true;
        }

        public void Disable () {
            highestValue = 0;
            enabled = false;
        }

        public bool CheckSell (double currentPrice) {
            if (enabled) {
                if (currentPrice > highestValue) highestValue = currentPrice;
                else if (highestValue * lossMargin > currentPrice) return true;
                return false;
            }
            return false;
        }

    }
}

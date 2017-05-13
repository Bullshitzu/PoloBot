using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utility;

namespace PoloniexBot.Windows {
    public partial class TickerFeedWindow : FormCustom {
        public TickerFeedWindow () {
            InitializeComponent();
            _ID = "TickerFeedWindow";
        }

        Dictionary<PoloniexAPI.CurrencyPair, double> LastValues;

        internal void RecieveMessage (object sender, PoloniexAPI.TickerChangedEventArgs e) {
            // try {
                e.Timestamp = DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);

                double lastPrice;
                if (LastValues == null) LastValues = new Dictionary<PoloniexAPI.CurrencyPair, double>();
                if (!LastValues.TryGetValue(e.CurrencyPair, out lastPrice)) lastPrice = e.MarketData.PriceLast;

                double change = e.MarketData.PriceLast - lastPrice;
                change /= lastPrice;
                change *= 100f;
                e.ChangeLast = change;
                LastValues.Remove(e.CurrencyPair);
                LastValues.Add(e.CurrencyPair, e.MarketData.PriceLast);

                tickerFeed.UpdateTicker(e);
                Data.Store.AddTickerData(e);
            // }
            // catch (Exception ex) {
            //    Console.WriteLine("EXCEPTION (RecieveMessage): " + ex.Message);
            // }
        }

        public void UpdateMarketData () {
            tickerFeed.UpdateMarketData();
        }

        private void refreshTimer_Tick (object sender, EventArgs e) {
            tickerFeed.Sort();
        }
    }
}

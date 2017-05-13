using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace PoloniexBot.Windows {
    public partial class ChartWindow : Form {
        public ChartWindow () {
            InitializeComponent();
            selectedPair = PoloniexAPI.CurrencyPair.Parse("BTC_XMR");
            selectedPeriod = PoloniexAPI.MarketTools.MarketPeriod.Minutes5;
        }

        public PoloniexAPI.CurrencyPair selectedPair;
        PoloniexAPI.MarketTools.MarketPeriod selectedPeriod;
        Thread thread;

        public void Start () {
            thread = Utility.ThreadManager.Register(Run, "Chart Update", true);
        }
        public void Stop () {
            Utility.ThreadManager.Kill(thread);
        }

        void Run () {
            while (true) {
                UpdateChart(selectedPair);

                Utility.ThreadManager.ReportAlive();
                Thread.Sleep(3000);
            }
        }

        public void UpdateChart (PoloniexAPI.CurrencyPair pair) {
            selectedPair = pair;
            IList<PoloniexAPI.MarketTools.IMarketChartData> chartData =
                ClientManager.RefreshChart(pair, selectedPeriod);

            if (chartData != null && selectedPair == pair) {
                chart.UpdateChartData(chartData, pair, selectedPeriod);
            }
        }
    }
}

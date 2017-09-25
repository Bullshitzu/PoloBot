using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.Windows.Controls {
    public partial class PredictorsScreen : MultiThreadControl {
        public PredictorsScreen () {
            InitializeComponent();
        }

        public PoloniexAPI.CurrencyPair selectedPair;

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Predictors";

            g.Clear(BackColor);

        }
    }
}

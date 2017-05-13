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

            if (selectedPair == null) return;
            Trading.TPManager tpmanager = Trading.Manager.GetTPManager(selectedPair);
            if (tpmanager == null) return;

            Data.Predictor[] predictors = tpmanager.GetPredictors();
            if (predictors == null) return;

            for (int i = 0; i < predictors.Length; i++) {
                if (predictors[i] == null) continue;

                try {
                    predictors[i].DrawPredictor(g, Data.Predictor.drawTimeframe, new RectangleF(0, 0, Size.Width, Size.Height));
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }
            }
        }
    }
}

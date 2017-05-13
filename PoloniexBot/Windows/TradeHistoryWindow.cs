using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.Windows {
    public partial class TradeHistoryWindow : FormCustom {
        public TradeHistoryWindow () {
            InitializeComponent();
            _ID = "TradeHistoryWindow";
        }
    }
}

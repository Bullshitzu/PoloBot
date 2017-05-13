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
    public partial class AccountStatusWindow : FormCustom {
        public AccountStatusWindow () {
            InitializeComponent();
            _ID = "WalletWindow";
        }

        public void UpdateBalance (IDictionary<string, PoloniexAPI.WalletTools.IBalance> wallet) {
            if (wallet == null) return;
            walletScreen.UpdateBalance(wallet);
        }

        public void UpdateOrderCount (int orderCount) {
            walletScreen.UpdateOrderCount(orderCount);
        }
    }
}

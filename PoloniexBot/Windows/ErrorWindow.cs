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
    public partial class ErrorWindow : FormCustom {
        public ErrorWindow () {
            InitializeComponent();
            _ID = "ErrorWindow";
        }

        public void ShowError (string text) {
            label.Text = text;
            label.ForeColor = Color.Red;
            Visible = true;
        }

        public void ShowResolved (string text) {
            label.Text = text;
            label.ForeColor = Color.Green;
        }

        private void label_Click (object sender, EventArgs e) {
            this.Visible = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.Windows {
    public class FormCustom : Form {

        protected string _ID;
        public string ID { get { return _ID; } }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

    }
}

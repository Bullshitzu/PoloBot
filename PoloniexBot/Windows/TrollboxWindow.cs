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
    public partial class TrollboxWindow : FormCustom {
        public TrollboxWindow () {
            InitializeComponent();
            _ID = "TrollboxWindow";
        }

        internal void RecieveMessage (object sender, PoloniexAPI.TrollboxMessageEventArgs e) {
            chatbox.Messages.Insert(0, e);
            chatbox.Invalidate();
        }

        Point dragPoint;
        private void pbDrag_MouseDown (object sender, MouseEventArgs e) {
            dragPoint = new Point(this.Location.X - MousePosition.X, this.Location.Y - MousePosition.Y);
            dragTimer.Enabled = true;
        }

        private void pbDrag_MouseUp (object sender, MouseEventArgs e) {
            dragTimer.Enabled = false;
        }

        private void dragTimer_Tick (object sender, EventArgs e) {
            this.Location = new Point(dragPoint.X + MousePosition.X, dragPoint.Y + MousePosition.Y);
        }

    }
}

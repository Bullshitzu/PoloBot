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
    public partial class ConsoleWindow : FormCustom {
        public ConsoleWindow () {
            InitializeComponent();
            _ID = "ConsoleWindow";
        }

        public void SetMessages (CLI.Manager.Message[] messages) {
            tbMain.Messages = messages;
            tbMain.Invalidate();
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

        private void tbInput_KeyDown (object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                CLI.Manager.ProcessInput(tbInput.Text);
                tbInput.Text = "";
            }
            else if (e.KeyCode == Keys.Up) {
                tbInput.Text = CLI.Manager.GetCommandUp();
            }
            else if (e.KeyCode == Keys.Down) {
                tbInput.Text = CLI.Manager.GetCommandDown();
            }
        }
    }
}

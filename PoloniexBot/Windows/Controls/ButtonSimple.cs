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
    public partial class ButtonSimple : Label {
        public ButtonSimple () {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
        }

        public void AssignGroup (string groupName) {
            toggleGroup = groupName;
            if (toggleGroup != null && toggleGroup != "") {
                if (ToggleGroups == null) return;

                List<ButtonSimple> thisGroup;
                if (ToggleGroups.TryGetValue(toggleGroup, out thisGroup)) {
                    if (!thisGroup.Contains(this)) thisGroup.Add(this);
                }
                else {
                    thisGroup = new List<ButtonSimple>();
                    thisGroup.Add(this);
                    ToggleGroups.Add(toggleGroup, thisGroup);
                }
            }
        }

        static ButtonSimple () {
            ToggleGroups = new Dictionary<string, List<ButtonSimple>>();
        }

        private static Dictionary<string, List<ButtonSimple>> ToggleGroups;

        private bool selected = false;
        private string toggleGroup = "test";

        private static Color colorNormal = Color.FromArgb(255, 0, 0, 0);
        private static Color colorHover = Color.FromArgb(255, 0, 64, 128);
        private static Color colorMousedown = Color.FromArgb(255, 0, 192, 255);
        private static Color colorSelected = Color.FromArgb(255, 0, 128, 128);

        private void ButtonSimple_MouseEnter (object sender, EventArgs e) {
            BackColor = colorHover;
        }

        private void ButtonSimple_MouseLeave (object sender, EventArgs e) {
            BackColor = selected ? colorSelected : colorNormal;
        }

        private void ButtonSimple_MouseDown (object sender, MouseEventArgs e) {
            BackColor = colorMousedown;
        }

        private void ButtonSimple_MouseUp (object sender, MouseEventArgs e) {
            BackColor = selected ? colorSelected : colorNormal;
        }

        public void SelectToggle() {
            if (toggleGroup == null || toggleGroup == "") return;
            if (ToggleGroups == null) return;

            List<ButtonSimple> thisGroup;
            if (ToggleGroups.TryGetValue(toggleGroup, out thisGroup)) {
                for (int i = 0; i < thisGroup.Count; i++) {
                    thisGroup[i].selected = false;
                    thisGroup[i].BackColor = colorNormal;
                }
            }

            this.selected = true;
            BackColor = colorSelected;
        }
    }
}

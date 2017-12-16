using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.GUI.Templates {
    class BaseButton : BaseControl {
        public BaseButton ()
            : base() {

            _ShowText = true;
            _ShowIcon = true;

            SetEvents();
        }
        public BaseButton (string text)
            : base() {

            Text = text;

            _ShowText = true;
            _ShowIcon = false;
            _Icon = IconType.Empty;

            SetEvents();
        }
        public BaseButton (IconType icon)
            : base() {

            Text = "";

            _ShowText = false;
            _ShowIcon = true;
            _Icon = icon;

            SetEvents();
        }

        private void InitializeComponent () {
            this.SuspendLayout();
            // 
            // BaseButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Location = new System.Drawing.Point(0, 1);
            this.Name = "BaseButton";
            this.Size = new System.Drawing.Size(220, 229);
            this.ResumeLayout(false);

        }

        private void SetEvents () {
            MouseEnter += BaseButton_MouseEnter;
            MouseLeave += BaseButton_MouseLeave;
            MouseDown += BaseButton_MouseDown;
            MouseUp += BaseButton_MouseUp;
        }

        Color BaseColor = Style.Colors.Primary.Light1;
        Color ButtonColor = Style.Colors.Primary.Light1;

        float ColorMultiplier = 1;
        float ColorDefault = 1;
        float ColorMouseover = 1.75f;
        float ColorClick = 2f;

        #region Mouse Interaction
        void BaseButton_MouseUp (object sender, MouseEventArgs e) {
            ColorMultiplier = ColorMouseover;
            ButtonColor = Helper.MultiplyColor(BaseColor, ColorMouseover);
            Toggle();
            Invalidate();
        }
        void BaseButton_MouseDown (object sender, MouseEventArgs e) {
            ColorMultiplier = ColorClick;
            ButtonColor = Helper.MultiplyColor(BaseColor, ColorClick);
            Invalidate();
        }
        void BaseButton_MouseLeave (object sender, EventArgs e) {
            ColorMultiplier = ColorDefault;
            ButtonColor = Helper.MultiplyColor(BaseColor, ColorDefault);
            Invalidate();
        }
        void BaseButton_MouseEnter (object sender, EventArgs e) {
            ColorMultiplier = ColorMouseover;
            ButtonColor = Helper.MultiplyColor(BaseColor, ColorMouseover);
            Invalidate();
        }
        #endregion

        private void Toggle () {
            _ToggleState = !_ToggleState;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public bool ToggleState {
            get {
                return _ToggleState;
            }

            set {
                _ToggleState = value;
            }
        }
        private bool _ToggleState = false;

        #region Display Properties
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text {
            get {
                return base.Text;
            }

            set {
                base.Text = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public IconType Icon {
            get {
                return _Icon;
            }
            set {
                _Icon = value;
            }
        }

        public enum IconType {
            Empty,
            Close,
            Forbid,
            Normal,
        }

        // This will be set by the constructors
        private bool _ShowText;
        private bool _ShowIcon;
        private IconType _Icon;
        #endregion

        protected override void OnPaint (System.Windows.Forms.PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (_ShowIcon) {
                switch (_ToggleState) {
                    case true:
                        DrawIconNormal(g);
                        break;
                    case false:
                        DrawIconForbid(g);
                        break;
                }
            }

            // note: border drawing is defined in DrawIcon implementations
        }

        protected new void DrawBorders (Graphics g, Color color, float thickness = 1) {
            float borderOffset = thickness / 2;
            using (Pen pen = new Pen(color, thickness)) {
                g.DrawLine(pen, borderOffset, borderOffset, Width - borderOffset, borderOffset);
                g.DrawLine(pen, borderOffset, borderOffset, borderOffset, Height - borderOffset);
                g.DrawLine(pen, Width - borderOffset, borderOffset, Width - borderOffset, Height - borderOffset);
                g.DrawLine(pen, borderOffset, Height - borderOffset, Width - borderOffset, Height - borderOffset);
            }
        }

        #region Icons
        private void DrawIconClose (Graphics g) {

            // Finally, draw the borders of the control
            // note: uses a custom implementation for mouseover reactions and such
            Color borderColor = Helper.MultiplyColor(Style.Colors.Secondary.Dark1, ColorMultiplier);

            if (_ShowText) {
                // Draw the text
                float width = g.MeasureString(Text, Font).Width;
                float height = Font.Height;

                using (Brush brush = new SolidBrush(borderColor)) {
                    g.DrawString(Text, Style.Fonts.Tiny, brush, new PointF((Width / 2) - (width / 2) + 1, (Height / 2) - (height / 2) + 1));
                }
            }

            float borderOffsetX = Width / 4f;
            float borderOffsetY = Height / 4f;

            // Draw the X
            using (Pen pen = new Pen(ButtonColor, 1.8f)) {
                g.DrawLine(pen, borderOffsetX, borderOffsetY, Width - borderOffsetX, Height - borderOffsetY);
                g.DrawLine(pen, borderOffsetX, Height - borderOffsetY, Width - borderOffsetX, borderOffsetY);
            }

            DrawBorders(g, borderColor, 2);
        }
        private void DrawIconForbid (Graphics g) {

            // Finally, draw the borders of the control
            // note: uses a custom implementation for mouseover reactions and such
            Color borderColor = Helper.MultiplyColor(Style.Colors.Secondary.Dark1, ColorMultiplier);

            if (_ShowText) {
                // Draw the text
                float width = g.MeasureString(Text, Font).Width;
                float height = Font.Height;

                using (Brush brush = new SolidBrush(borderColor)) {
                    g.DrawString(Text, Style.Fonts.Tiny, brush, new PointF((Width / 2) - (width / 2) + 1, (Height / 2) - (height / 2) + 1));
                }
            }

            DrawBorders(g, borderColor, 1);
        }
        private void DrawIconNormal (Graphics g) {

            // Finally, draw the borders of the control
            // note: uses a custom implementation for mouseover reactions and such
            Color borderColor = Helper.MultiplyColor(Style.Colors.Primary.Main, ColorMultiplier);

            if (_ShowText) {
                // Draw the text
                float width = g.MeasureString(Text, Font).Width;
                float height = Font.Height;

                using (Brush brush = new SolidBrush(borderColor)) {
                    g.DrawString(Text, Style.Fonts.Tiny, brush, new PointF((Width / 2) - (width / 2) + 1, (Height / 2) - (height / 2) + 1));
                }
            }


            DrawBorders(g, borderColor, 1);
        }
        #endregion
    }
}

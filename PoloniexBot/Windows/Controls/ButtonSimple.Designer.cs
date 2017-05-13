namespace PoloniexBot.Windows.Controls {
    partial class ButtonSimple {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.SuspendLayout();
            // 
            // ButtonSimple
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.Name = "ButtonSimple";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ButtonSimple_MouseDown);
            this.MouseEnter += new System.EventHandler(this.ButtonSimple_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ButtonSimple_MouseLeave);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ButtonSimple_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

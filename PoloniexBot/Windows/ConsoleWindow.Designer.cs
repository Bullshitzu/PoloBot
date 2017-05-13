namespace PoloniexBot.Windows {
    partial class ConsoleWindow {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConsoleWindow));
            this.tbInput = new System.Windows.Forms.TextBox();
            this.pbDrag = new System.Windows.Forms.PictureBox();
            this.dragTimer = new System.Windows.Forms.Timer(this.components);
            this.tbMain = new PoloniexBot.Windows.Controls.ConsoleScreen();
            ((System.ComponentModel.ISupportInitialize)(this.pbDrag)).BeginInit();
            this.SuspendLayout();
            // 
            // tbInput
            // 
            this.tbInput.BackColor = System.Drawing.Color.Black;
            this.tbInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbInput.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tbInput.Location = new System.Drawing.Point(6, 285);
            this.tbInput.Name = "tbInput";
            this.tbInput.Size = new System.Drawing.Size(848, 27);
            this.tbInput.TabIndex = 0;
            this.tbInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbInput_KeyDown);
            // 
            // pbDrag
            // 
            this.pbDrag.BackColor = System.Drawing.Color.Transparent;
            this.pbDrag.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbDrag.BackgroundImage")));
            this.pbDrag.Location = new System.Drawing.Point(414, 15);
            this.pbDrag.Name = "pbDrag";
            this.pbDrag.Size = new System.Drawing.Size(32, 32);
            this.pbDrag.TabIndex = 2;
            this.pbDrag.TabStop = false;
            this.pbDrag.Visible = false;
            this.pbDrag.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbDrag_MouseDown);
            this.pbDrag.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbDrag_MouseUp);
            // 
            // dragTimer
            // 
            this.dragTimer.Interval = 15;
            this.dragTimer.Tick += new System.EventHandler(this.dragTimer_Tick);
            // 
            // tbMain
            // 
            this.tbMain.BackColor = System.Drawing.Color.Black;
            this.tbMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbMain.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tbMain.Location = new System.Drawing.Point(4, 4);
            this.tbMain.Name = "tbMain";
            this.tbMain.Size = new System.Drawing.Size(850, 276);
            this.tbMain.TabIndex = 1;
            // 
            // ConsoleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(860, 320);
            this.Controls.Add(this.pbDrag);
            this.Controls.Add(this.tbMain);
            this.Controls.Add(this.tbInput);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConsoleWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ConsoleWindow";
            this.TransparencyKey = System.Drawing.SystemColors.MenuHighlight;
            ((System.ComponentModel.ISupportInitialize)(this.pbDrag)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbInput;
        public Windows.Controls.ConsoleScreen tbMain;
        public System.Windows.Forms.PictureBox pbDrag;
        private System.Windows.Forms.Timer dragTimer;
    }
}

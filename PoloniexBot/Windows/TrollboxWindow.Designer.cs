namespace PoloniexBot.Windows {
    partial class TrollboxWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrollboxWindow));
            this.pbDrag = new System.Windows.Forms.PictureBox();
            this.dragTimer = new System.Windows.Forms.Timer(this.components);
            this.chatbox = new PoloniexBot.Windows.Controls.Chatbox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDrag)).BeginInit();
            this.SuspendLayout();
            // 
            // pbDrag
            // 
            this.pbDrag.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pbDrag.BackColor = System.Drawing.Color.Transparent;
            this.pbDrag.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbDrag.BackgroundImage")));
            this.pbDrag.Location = new System.Drawing.Point(234, 15);
            this.pbDrag.Name = "pbDrag";
            this.pbDrag.Size = new System.Drawing.Size(32, 32);
            this.pbDrag.TabIndex = 0;
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
            // chatbox
            // 
            this.chatbox.BackColor = System.Drawing.Color.Transparent;
            this.chatbox.Font = new System.Drawing.Font("Calibri Bold Caps", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.chatbox.Location = new System.Drawing.Point(4, 4);
            this.chatbox.Name = "chatbox";
            this.chatbox.Size = new System.Drawing.Size(492, 312);
            this.chatbox.TabIndex = 1;
            this.chatbox.Text = "chatbox";
            // 
            // TrollboxWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(500, 320);
            this.Controls.Add(this.pbDrag);
            this.Controls.Add(this.chatbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TrollboxWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TrollboxWindow";
            this.TransparencyKey = System.Drawing.SystemColors.MenuHighlight;
            ((System.ComponentModel.ISupportInitialize)(this.pbDrag)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox pbDrag;
        private System.Windows.Forms.Timer dragTimer;
        private Controls.Chatbox chatbox;
    }
}

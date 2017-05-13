namespace PoloniexBot.Windows {
    partial class TickerFeedWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TickerFeedWindow));
            this.pbDrag = new System.Windows.Forms.PictureBox();
            this.dragTimer = new System.Windows.Forms.Timer(this.components);
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.tickerFeed = new PoloniexBot.Windows.Controls.TickerFeed();
            ((System.ComponentModel.ISupportInitialize)(this.pbDrag)).BeginInit();
            this.SuspendLayout();
            // 
            // pbDrag
            // 
            this.pbDrag.BackColor = System.Drawing.Color.Transparent;
            this.pbDrag.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbDrag.BackgroundImage")));
            this.pbDrag.Location = new System.Drawing.Point(184, 15);
            this.pbDrag.Name = "pbDrag";
            this.pbDrag.Size = new System.Drawing.Size(32, 32);
            this.pbDrag.TabIndex = 0;
            this.pbDrag.TabStop = false;
            this.pbDrag.Visible = false;
            // 
            // dragTimer
            // 
            this.dragTimer.Interval = 15;
            // 
            // refreshTimer
            // 
            this.refreshTimer.Enabled = true;
            this.refreshTimer.Interval = 15;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // tickerFeed
            // 
            this.tickerFeed.BackColor = System.Drawing.Color.Transparent;
            this.tickerFeed.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tickerFeed.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.tickerFeed.Location = new System.Drawing.Point(4, 4);
            this.tickerFeed.Name = "tickerFeed";
            this.tickerFeed.Size = new System.Drawing.Size(392, 992);
            this.tickerFeed.TabIndex = 1;
            this.tickerFeed.Text = "tickerFeed";
            // 
            // TickerFeedWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(400, 1000);
            this.Controls.Add(this.pbDrag);
            this.Controls.Add(this.tickerFeed);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TickerFeedWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "TickerFeedWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pbDrag)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox pbDrag;
        private System.Windows.Forms.Timer dragTimer;
        public Controls.TickerFeed tickerFeed;
        private System.Windows.Forms.Timer refreshTimer;
    }
}

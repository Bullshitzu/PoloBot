namespace PoloniexBot.Windows {
    partial class TradeHistoryWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TradeHistoryWindow));
            this.tradeHistoryScreen = new PoloniexBot.Windows.Controls.TradeHistoryScreen();
            this.SuspendLayout();
            // 
            // tradeHistoryScreen
            // 
            this.tradeHistoryScreen.BackColor = System.Drawing.Color.Black;
            this.tradeHistoryScreen.Location = new System.Drawing.Point(4, 4);
            this.tradeHistoryScreen.Name = "tradeHistoryScreen";
            this.tradeHistoryScreen.Size = new System.Drawing.Size(852, 272);
            this.tradeHistoryScreen.TabIndex = 0;
            // 
            // TradeHistoryWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(292, 270);
            this.ControlBox = false;
            this.Controls.Add(this.tradeHistoryScreen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TradeHistoryWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "TradeHistoryWindow";
            this.ResumeLayout(false);

        }

        #endregion

        public Controls.TradeHistoryScreen tradeHistoryScreen;


    }
}

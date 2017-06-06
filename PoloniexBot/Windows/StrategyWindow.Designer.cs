namespace PoloniexBot.Windows {
    partial class StrategyWindow {
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
            this.strategyScreen = new PoloniexBot.Windows.Controls.StrategyScreen();
            this.SuspendLayout();
            // 
            // strategyScreen1
            // 
            this.strategyScreen.Location = new System.Drawing.Point(4, 4);
            this.strategyScreen.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.strategyScreen.Name = "strategyScreen1";
            this.strategyScreen.Size = new System.Drawing.Size(852, 312);
            this.strategyScreen.TabIndex = 0;
            // 
            // StrategyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(860, 320);
            this.Controls.Add(this.strategyScreen);
            this.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "StrategyWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "StrategyWindow";
            this.ResumeLayout(false);

        }

        #endregion

        public Controls.StrategyScreen strategyScreen;
    }
}

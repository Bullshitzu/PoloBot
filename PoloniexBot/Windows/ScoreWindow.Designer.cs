namespace PoloniexBot.Windows {
    partial class ScoreWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScoreWindow));
            this.scoreScreen = new PoloniexBot.Windows.Controls.ScoreScreen();
            this.SuspendLayout();
            // 
            // scoreScreen
            // 
            this.scoreScreen.BackColor = System.Drawing.Color.Transparent;
            this.scoreScreen.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.scoreScreen.Location = new System.Drawing.Point(4, 4);
            this.scoreScreen.Name = "scoreScreen";
            this.scoreScreen.Size = new System.Drawing.Size(492, 312);
            this.scoreScreen.TabIndex = 0;
            this.scoreScreen.Text = "scoreScreen1";
            // 
            // ScoreWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(500, 320);
            this.Controls.Add(this.scoreScreen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ScoreWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ScoreWindow";
            this.ResumeLayout(false);

        }

        #endregion

        public Controls.ScoreScreen scoreScreen;
    }
}

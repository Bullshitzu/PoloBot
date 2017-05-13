namespace PoloniexBot.Windows {
    partial class PerformanceWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformanceWindow));
            this.performanceScreen = new PoloniexBot.Windows.Controls.PerformanceScreen();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // performanceScreen
            // 
            this.performanceScreen.BackColor = System.Drawing.Color.Transparent;
            this.performanceScreen.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.performanceScreen.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.performanceScreen.Location = new System.Drawing.Point(4, 4);
            this.performanceScreen.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.performanceScreen.Name = "performanceScreen";
            this.performanceScreen.Size = new System.Drawing.Size(492, 312);
            this.performanceScreen.TabIndex = 1;
            // 
            // PerformanceWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(500, 320);
            this.Controls.Add(this.performanceScreen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PerformanceWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "PerformanceWindow";
            this.TransparencyKey = System.Drawing.SystemColors.MenuHighlight;
            this.ResumeLayout(false);

        }

        #endregion

        public Controls.PerformanceScreen performanceScreen;
        private System.Windows.Forms.Timer refreshTimer;
    }
}

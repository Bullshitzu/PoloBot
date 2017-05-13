namespace PoloniexBot.Windows {
    partial class PredictorsWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PredictorsWindow));
            this.predictorsScreen = new PoloniexBot.Windows.Controls.PredictorsScreen();
            this.SuspendLayout();
            // 
            // predictorsScreen
            // 
            this.predictorsScreen.BackColor = System.Drawing.Color.Transparent;
            this.predictorsScreen.Location = new System.Drawing.Point(4, 4);
            this.predictorsScreen.Name = "predictorsScreen";
            this.predictorsScreen.Size = new System.Drawing.Size(852, 272);
            this.predictorsScreen.TabIndex = 0;
            // 
            // PredictorsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(860, 280);
            this.ControlBox = false;
            this.Controls.Add(this.predictorsScreen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PredictorsWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "PredictorsWindow";
            this.ResumeLayout(false);

        }

        #endregion

        public Windows.Controls.PredictorsScreen predictorsScreen;
    }
}

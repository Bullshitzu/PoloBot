namespace PoloniexBot.Windows {
    partial class AccountStatusWindow {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountStatusWindow));
            this.walletScreen = new PoloniexBot.Windows.Controls.WalletScreen();
            this.SuspendLayout();
            // 
            // walletScreen
            // 
            this.walletScreen.BackColor = System.Drawing.Color.Black;
            this.walletScreen.Location = new System.Drawing.Point(4, 4);
            this.walletScreen.Name = "walletScreen";
            this.walletScreen.Size = new System.Drawing.Size(492, 272);
            this.walletScreen.TabIndex = 0;
            this.walletScreen.Text = "label1";
            // 
            // AccountStatusWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(500, 280);
            this.Controls.Add(this.walletScreen);
            this.Font = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AccountStatusWindow";
            this.Opacity = 0.75D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "OwnedAmountsWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private Windows.Controls.WalletScreen walletScreen;

    }
}

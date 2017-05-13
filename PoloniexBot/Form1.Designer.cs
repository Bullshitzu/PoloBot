namespace PoloniexBot {
    partial class Form1 {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnShowAnalyzer = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowTradeFeed = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowOwnedAmounts = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowTrollbox = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowTickerFeed = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowAll = new System.Windows.Forms.ToolStripMenuItem();
            this.btnHideAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnToggleMove = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSavePositions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExit = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "CryptoBot";
            this.notifyIcon.Visible = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnShowAnalyzer,
            this.btnShowTradeFeed,
            this.btnShowOwnedAmounts,
            this.btnShowTrollbox,
            this.btnShowTickerFeed,
            this.btnShowConsole,
            this.toolStripSeparator2,
            this.toolStripMenuItem1,
            this.toolStripSeparator1,
            this.btnExit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(187, 192);
            // 
            // btnShowAnalyzer
            // 
            this.btnShowAnalyzer.Name = "btnShowAnalyzer";
            this.btnShowAnalyzer.Size = new System.Drawing.Size(186, 22);
            this.btnShowAnalyzer.Text = "Show Analyzer";
            // 
            // btnShowTradeFeed
            // 
            this.btnShowTradeFeed.Name = "btnShowTradeFeed";
            this.btnShowTradeFeed.Size = new System.Drawing.Size(186, 22);
            this.btnShowTradeFeed.Text = "Show Trade Feed";
            // 
            // btnShowOwnedAmounts
            // 
            this.btnShowOwnedAmounts.Name = "btnShowOwnedAmounts";
            this.btnShowOwnedAmounts.Size = new System.Drawing.Size(186, 22);
            this.btnShowOwnedAmounts.Text = "Show Owned Amounts";
            // 
            // btnShowTrollbox
            // 
            this.btnShowTrollbox.Name = "btnShowTrollbox";
            this.btnShowTrollbox.Size = new System.Drawing.Size(186, 22);
            this.btnShowTrollbox.Text = "Show Trollbox";
            // 
            // btnShowTickerFeed
            // 
            this.btnShowTickerFeed.Name = "btnShowTickerFeed";
            this.btnShowTickerFeed.Size = new System.Drawing.Size(186, 22);
            this.btnShowTickerFeed.Text = "Show Ticker Feed";
            // 
            // btnShowConsole
            // 
            this.btnShowConsole.Name = "btnShowConsole";
            this.btnShowConsole.Size = new System.Drawing.Size(186, 22);
            this.btnShowConsole.Text = "Show Console";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(183, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnShowAll,
            this.btnHideAll,
            this.toolStripSeparator3,
            this.btnToggleMove,
            this.btnSavePositions});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(186, 22);
            this.toolStripMenuItem1.Text = "Windows";
            // 
            // btnShowAll
            // 
            this.btnShowAll.Name = "btnShowAll";
            this.btnShowAll.Size = new System.Drawing.Size(145, 22);
            this.btnShowAll.Text = "Show All";
            // 
            // btnHideAll
            // 
            this.btnHideAll.Name = "btnHideAll";
            this.btnHideAll.Size = new System.Drawing.Size(145, 22);
            this.btnHideAll.Text = "Hide All";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(142, 6);
            // 
            // btnToggleMove
            // 
            this.btnToggleMove.Name = "btnToggleMove";
            this.btnToggleMove.Size = new System.Drawing.Size(145, 22);
            this.btnToggleMove.Text = "Toggle Move";
            // 
            // btnSavePositions
            // 
            this.btnSavePositions.Name = "btnSavePositions";
            this.btnSavePositions.Size = new System.Drawing.Size(145, 22);
            this.btnSavePositions.Text = "Save Positions";
            this.btnSavePositions.Click += new System.EventHandler(this.btnSavePositions_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
            // 
            // btnExit
            // 
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(186, 22);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.ClientSize = new System.Drawing.Size(292, 270);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.TransparencyKey = System.Drawing.SystemColors.MenuHighlight;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem btnShowTradeFeed;
        private System.Windows.Forms.ToolStripMenuItem btnShowOwnedAmounts;
        private System.Windows.Forms.ToolStripMenuItem btnShowTrollbox;
        private System.Windows.Forms.ToolStripMenuItem btnShowConsole;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem btnExit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem btnShowAll;
        private System.Windows.Forms.ToolStripMenuItem btnHideAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem btnToggleMove;
        private System.Windows.Forms.ToolStripMenuItem btnSavePositions;
        private System.Windows.Forms.ToolStripMenuItem btnShowTickerFeed;
        private System.Windows.Forms.ToolStripMenuItem btnShowAnalyzer;
    }
}


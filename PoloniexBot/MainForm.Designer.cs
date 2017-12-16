namespace PoloniexBot {
    partial class MainForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tbConsoleInput = new System.Windows.Forms.TextBox();
            this.lblConsoleInput = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.bringToFrontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.baseButton5 = new PoloniexBot.GUI.Templates.BaseButton();
            this.baseButton4 = new PoloniexBot.GUI.Templates.BaseButton();
            this.baseButton3 = new PoloniexBot.GUI.Templates.BaseButton();
            this.baseButton2 = new PoloniexBot.GUI.Templates.BaseButton();
            this.baseButton1 = new PoloniexBot.GUI.Templates.BaseButton();
            this.strategyControl1 = new PoloniexBot.GUI.StrategyControl();
            this.mainSummaryGraph1 = new PoloniexBot.GUI.MainSummaryGraph();
            this.walletControl1 = new PoloniexBot.GUI.WalletControl();
            this.consoleControl1 = new PoloniexBot.GUI.ConsoleControl();
            this.memoryControl1 = new PoloniexBot.GUI.MemoryControl();
            this.apiCallsControl1 = new PoloniexBot.GUI.ApiCallsControl();
            this.tradeHistoryControl1 = new PoloniexBot.GUI.TradeHistoryControl();
            this.pairSummaryControl12 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl11 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl10 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl7 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl8 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl9 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl4 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl5 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl6 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl3 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl2 = new PoloniexBot.GUI.PairSummaryControl();
            this.pairSummaryControl1 = new PoloniexBot.GUI.PairSummaryControl();
            this.pingControl1 = new PoloniexBot.GUI.PingControl();
            this.networkGraph1 = new PoloniexBot.GUI.NetworkGraph();
            this.threadsControl1 = new PoloniexBot.GUI.ThreadsControl();
            this.cpuGraph4 = new PoloniexBot.GUI.CPUGraph();
            this.cpuGraph3 = new PoloniexBot.GUI.CPUGraph();
            this.cpuGraph2 = new PoloniexBot.GUI.CPUGraph();
            this.cpuGraph1 = new PoloniexBot.GUI.CPUGraph();
            this.statusControl1 = new PoloniexBot.GUI.StatusControl();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 10;
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 10;
            this.toolTip.ReshowDelay = 2;
            this.toolTip.ShowAlways = true;
            this.toolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // tbConsoleInput
            // 
            this.tbConsoleInput.BackColor = System.Drawing.Color.Black;
            this.tbConsoleInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbConsoleInput.Font = GUI.Style.Fonts.Reduced;
            this.tbConsoleInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(133)))), ((int)(((byte)(192)))));
            this.tbConsoleInput.Location = new System.Drawing.Point(503, 1012);
            this.tbConsoleInput.Name = "tbConsoleInput";
            this.tbConsoleInput.Size = new System.Drawing.Size(100, 20);
            this.tbConsoleInput.TabIndex = 36;
            this.tbConsoleInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbConsoleInput_KeyDown);
            // 
            // lblConsoleInput
            // 
            this.lblConsoleInput.AutoSize = true;
            this.lblConsoleInput.Location = new System.Drawing.Point(519, 966);
            this.lblConsoleInput.Name = "lblConsoleInput";
            this.lblConsoleInput.Size = new System.Drawing.Size(42, 14);
            this.lblConsoleInput.TabIndex = 37;
            this.lblConsoleInput.Text = "user>";
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Poloniex Bot";
            this.notifyIcon.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bringToFrontToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(144, 48);
            // 
            // bringToFrontToolStripMenuItem
            // 
            this.bringToFrontToolStripMenuItem.Name = "bringToFrontToolStripMenuItem";
            this.bringToFrontToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.bringToFrontToolStripMenuItem.Text = "Bring To Front";
            this.bringToFrontToolStripMenuItem.Click += new System.EventHandler(this.bringToFrontToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // baseButton5
            // 
            this.baseButton5.Icon = PoloniexBot.GUI.Templates.BaseButton.IconType.Normal;
            this.baseButton5.Location = new System.Drawing.Point(989, 894);
            this.baseButton5.Margin = new System.Windows.Forms.Padding(1);
            this.baseButton5.Name = "baseButton5";
            this.baseButton5.Size = new System.Drawing.Size(121, 64);
            this.baseButton5.TabIndex = 35;
            this.baseButton5.Text = "Error";
            this.baseButton5.ToggleState = true;
            // 
            // baseButton4
            // 
            this.baseButton4.Icon = PoloniexBot.GUI.Templates.BaseButton.IconType.Forbid;
            this.baseButton4.Location = new System.Drawing.Point(818, 899);
            this.baseButton4.Margin = new System.Windows.Forms.Padding(1);
            this.baseButton4.Name = "baseButton4";
            this.baseButton4.Size = new System.Drawing.Size(104, 59);
            this.baseButton4.TabIndex = 34;
            this.baseButton4.Text = "Warning";
            this.baseButton4.ToggleState = false;
            // 
            // baseButton3
            // 
            this.baseButton3.Icon = PoloniexBot.GUI.Templates.BaseButton.IconType.Normal;
            this.baseButton3.Location = new System.Drawing.Point(699, 894);
            this.baseButton3.Margin = new System.Windows.Forms.Padding(1);
            this.baseButton3.Name = "baseButton3";
            this.baseButton3.Size = new System.Drawing.Size(89, 55);
            this.baseButton3.TabIndex = 33;
            this.baseButton3.Text = "Note";
            this.baseButton3.ToggleState = true;
            // 
            // baseButton2
            // 
            this.baseButton2.Icon = PoloniexBot.GUI.Templates.BaseButton.IconType.Normal;
            this.baseButton2.Location = new System.Drawing.Point(602, 894);
            this.baseButton2.Margin = new System.Windows.Forms.Padding(1);
            this.baseButton2.Name = "baseButton2";
            this.baseButton2.Size = new System.Drawing.Size(76, 51);
            this.baseButton2.TabIndex = 32;
            this.baseButton2.Text = "Log";
            this.baseButton2.ToggleState = true;
            // 
            // baseButton1
            // 
            this.baseButton1.Icon = PoloniexBot.GUI.Templates.BaseButton.IconType.Normal;
            this.baseButton1.Location = new System.Drawing.Point(503, 894);
            this.baseButton1.Margin = new System.Windows.Forms.Padding(1);
            this.baseButton1.Name = "baseButton1";
            this.baseButton1.Size = new System.Drawing.Size(65, 47);
            this.baseButton1.TabIndex = 31;
            this.baseButton1.Text = "User";
            this.baseButton1.ToggleState = true;
            // 
            // strategyControl1
            // 
            this.strategyControl1.Location = new System.Drawing.Point(422, 663);
            this.strategyControl1.Margin = new System.Windows.Forms.Padding(1);
            this.strategyControl1.Name = "strategyControl1";
            this.strategyControl1.Size = new System.Drawing.Size(783, 153);
            this.strategyControl1.TabIndex = 30;
            // 
            // mainSummaryGraph1
            // 
            this.mainSummaryGraph1.Location = new System.Drawing.Point(699, 524);
            this.mainSummaryGraph1.Margin = new System.Windows.Forms.Padding(1);
            this.mainSummaryGraph1.Name = "mainSummaryGraph1";
            this.mainSummaryGraph1.Size = new System.Drawing.Size(489, 113);
            this.mainSummaryGraph1.TabIndex = 29;
            // 
            // walletControl1
            // 
            this.walletControl1.Location = new System.Drawing.Point(412, 524);
            this.walletControl1.Margin = new System.Windows.Forms.Padding(1);
            this.walletControl1.Name = "walletControl1";
            this.walletControl1.Size = new System.Drawing.Size(245, 124);
            this.walletControl1.TabIndex = 28;
            // 
            // consoleControl1
            // 
            this.consoleControl1.Location = new System.Drawing.Point(412, 844);
            this.consoleControl1.Margin = new System.Windows.Forms.Padding(1);
            this.consoleControl1.Name = "consoleControl1";
            this.consoleControl1.Size = new System.Drawing.Size(840, 199);
            this.consoleControl1.TabIndex = 27;
            this.consoleControl1.MouseWheel += consoleControl1_MouseWheel;
            // 
            // memoryControl1
            // 
            this.memoryControl1.Location = new System.Drawing.Point(412, 280);
            this.memoryControl1.Margin = new System.Windows.Forms.Padding(1);
            this.memoryControl1.Name = "memoryControl1";
            this.memoryControl1.Size = new System.Drawing.Size(776, 231);
            this.memoryControl1.TabIndex = 26;
            // 
            // apiCallsControl1
            // 
            this.apiCallsControl1.Location = new System.Drawing.Point(9, 679);
            this.apiCallsControl1.Margin = new System.Windows.Forms.Padding(1);
            this.apiCallsControl1.Name = "apiCallsControl1";
            this.apiCallsControl1.Size = new System.Drawing.Size(322, 159);
            this.apiCallsControl1.TabIndex = 25;
            // 
            // tradeHistoryControl1
            // 
            this.tradeHistoryControl1.Location = new System.Drawing.Point(412, 11);
            this.tradeHistoryControl1.Margin = new System.Windows.Forms.Padding(1);
            this.tradeHistoryControl1.Name = "tradeHistoryControl1";
            this.tradeHistoryControl1.Size = new System.Drawing.Size(1017, 265);
            this.tradeHistoryControl1.TabIndex = 24;
            // 
            // pairSummaryControl12
            // 
            this.pairSummaryControl12.Location = new System.Drawing.Point(1460, 981);
            this.pairSummaryControl12.Margin = new System.Windows.Forms.Padding(11, 2, 11, 2);
            this.pairSummaryControl12.Name = "pairSummaryControl12";
            this.pairSummaryControl12.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl12.TabIndex = 21;
            // 
            // pairSummaryControl11
            // 
            this.pairSummaryControl11.Location = new System.Drawing.Point(1460, 894);
            this.pairSummaryControl11.Margin = new System.Windows.Forms.Padding(7, 2, 7, 2);
            this.pairSummaryControl11.Name = "pairSummaryControl11";
            this.pairSummaryControl11.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl11.TabIndex = 20;
            // 
            // pairSummaryControl10
            // 
            this.pairSummaryControl10.Location = new System.Drawing.Point(1460, 805);
            this.pairSummaryControl10.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.pairSummaryControl10.Name = "pairSummaryControl10";
            this.pairSummaryControl10.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl10.TabIndex = 19;
            // 
            // pairSummaryControl7
            // 
            this.pairSummaryControl7.Location = new System.Drawing.Point(1460, 629);
            this.pairSummaryControl7.Margin = new System.Windows.Forms.Padding(7, 2, 7, 2);
            this.pairSummaryControl7.Name = "pairSummaryControl7";
            this.pairSummaryControl7.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl7.TabIndex = 18;
            // 
            // pairSummaryControl8
            // 
            this.pairSummaryControl8.Location = new System.Drawing.Point(1460, 540);
            this.pairSummaryControl8.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.pairSummaryControl8.Name = "pairSummaryControl8";
            this.pairSummaryControl8.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl8.TabIndex = 17;
            // 
            // pairSummaryControl9
            // 
            this.pairSummaryControl9.Location = new System.Drawing.Point(1460, 717);
            this.pairSummaryControl9.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pairSummaryControl9.Name = "pairSummaryControl9";
            this.pairSummaryControl9.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl9.TabIndex = 16;
            // 
            // pairSummaryControl4
            // 
            this.pairSummaryControl4.Location = new System.Drawing.Point(1460, 365);
            this.pairSummaryControl4.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.pairSummaryControl4.Name = "pairSummaryControl4";
            this.pairSummaryControl4.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl4.TabIndex = 15;
            // 
            // pairSummaryControl5
            // 
            this.pairSummaryControl5.Location = new System.Drawing.Point(1460, 278);
            this.pairSummaryControl5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pairSummaryControl5.Name = "pairSummaryControl5";
            this.pairSummaryControl5.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl5.TabIndex = 14;
            // 
            // pairSummaryControl6
            // 
            this.pairSummaryControl6.Location = new System.Drawing.Point(1460, 453);
            this.pairSummaryControl6.Margin = new System.Windows.Forms.Padding(2);
            this.pairSummaryControl6.Name = "pairSummaryControl6";
            this.pairSummaryControl6.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl6.TabIndex = 13;
            // 
            // pairSummaryControl3
            // 
            this.pairSummaryControl3.Location = new System.Drawing.Point(1460, 189);
            this.pairSummaryControl3.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.pairSummaryControl3.Name = "pairSummaryControl3";
            this.pairSummaryControl3.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl3.TabIndex = 12;
            // 
            // pairSummaryControl2
            // 
            this.pairSummaryControl2.Location = new System.Drawing.Point(1460, 101);
            this.pairSummaryControl2.Margin = new System.Windows.Forms.Padding(2);
            this.pairSummaryControl2.Name = "pairSummaryControl2";
            this.pairSummaryControl2.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl2.TabIndex = 11;
            // 
            // pairSummaryControl1
            // 
            this.pairSummaryControl1.Location = new System.Drawing.Point(1460, 11);
            this.pairSummaryControl1.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.pairSummaryControl1.Name = "pairSummaryControl1";
            this.pairSummaryControl1.Size = new System.Drawing.Size(450, 86);
            this.pairSummaryControl1.TabIndex = 10;
            // 
            // pingControl1
            // 
            this.pingControl1.Location = new System.Drawing.Point(9, 553);
            this.pingControl1.Margin = new System.Windows.Forms.Padding(2);
            this.pingControl1.Name = "pingControl1";
            this.pingControl1.Size = new System.Drawing.Size(323, 121);
            this.pingControl1.TabIndex = 9;
            // 
            // networkGraph1
            // 
            this.networkGraph1.Location = new System.Drawing.Point(9, 430);
            this.networkGraph1.Margin = new System.Windows.Forms.Padding(2);
            this.networkGraph1.Name = "networkGraph1";
            this.networkGraph1.Size = new System.Drawing.Size(350, 121);
            this.networkGraph1.TabIndex = 8;
            // 
            // threadsControl1
            // 
            this.threadsControl1.Location = new System.Drawing.Point(9, 275);
            this.threadsControl1.Margin = new System.Windows.Forms.Padding(2);
            this.threadsControl1.Name = "threadsControl1";
            this.threadsControl1.Size = new System.Drawing.Size(323, 155);
            this.threadsControl1.TabIndex = 7;
            // 
            // cpuGraph4
            // 
            this.cpuGraph4.Location = new System.Drawing.Point(171, 168);
            this.cpuGraph4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cpuGraph4.Name = "cpuGraph4";
            this.cpuGraph4.Size = new System.Drawing.Size(160, 103);
            this.cpuGraph4.TabIndex = 6;
            this.cpuGraph4.Text = "CPU 04";
            // 
            // cpuGraph3
            // 
            this.cpuGraph3.Location = new System.Drawing.Point(9, 168);
            this.cpuGraph3.Margin = new System.Windows.Forms.Padding(2);
            this.cpuGraph3.Name = "cpuGraph3";
            this.cpuGraph3.Size = new System.Drawing.Size(160, 103);
            this.cpuGraph3.TabIndex = 5;
            this.cpuGraph3.Text = "CPU 03";
            // 
            // cpuGraph2
            // 
            this.cpuGraph2.Location = new System.Drawing.Point(171, 61);
            this.cpuGraph2.Margin = new System.Windows.Forms.Padding(2);
            this.cpuGraph2.Name = "cpuGraph2";
            this.cpuGraph2.Size = new System.Drawing.Size(160, 103);
            this.cpuGraph2.TabIndex = 4;
            this.cpuGraph2.Text = "CPU 02";
            // 
            // cpuGraph1
            // 
            this.cpuGraph1.Location = new System.Drawing.Point(9, 61);
            this.cpuGraph1.Margin = new System.Windows.Forms.Padding(2);
            this.cpuGraph1.Name = "cpuGraph1";
            this.cpuGraph1.Size = new System.Drawing.Size(160, 103);
            this.cpuGraph1.TabIndex = 3;
            this.cpuGraph1.Text = "CPU 01";
            // 
            // statusControl1
            // 
            this.statusControl1.Location = new System.Drawing.Point(9, 10);
            this.statusControl1.Margin = new System.Windows.Forms.Padding(2);
            this.statusControl1.Name = "statusControl1";
            this.statusControl1.Size = new System.Drawing.Size(400, 51);
            this.statusControl1.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1920, 1062);
            this.Controls.Add(this.lblConsoleInput);
            this.Controls.Add(this.tbConsoleInput);
            this.Controls.Add(this.baseButton5);
            this.Controls.Add(this.baseButton4);
            this.Controls.Add(this.baseButton3);
            this.Controls.Add(this.baseButton2);
            this.Controls.Add(this.baseButton1);
            this.Controls.Add(this.strategyControl1);
            this.Controls.Add(this.mainSummaryGraph1);
            this.Controls.Add(this.walletControl1);
            this.Controls.Add(this.consoleControl1);
            this.Controls.Add(this.memoryControl1);
            this.Controls.Add(this.apiCallsControl1);
            this.Controls.Add(this.tradeHistoryControl1);
            this.Controls.Add(this.pairSummaryControl12);
            this.Controls.Add(this.pairSummaryControl11);
            this.Controls.Add(this.pairSummaryControl10);
            this.Controls.Add(this.pairSummaryControl7);
            this.Controls.Add(this.pairSummaryControl8);
            this.Controls.Add(this.pairSummaryControl9);
            this.Controls.Add(this.pairSummaryControl4);
            this.Controls.Add(this.pairSummaryControl5);
            this.Controls.Add(this.pairSummaryControl6);
            this.Controls.Add(this.pairSummaryControl3);
            this.Controls.Add(this.pairSummaryControl2);
            this.Controls.Add(this.pairSummaryControl1);
            this.Controls.Add(this.pingControl1);
            this.Controls.Add(this.networkGraph1);
            this.Controls.Add(this.threadsControl1);
            this.Controls.Add(this.cpuGraph4);
            this.Controls.Add(this.cpuGraph3);
            this.Controls.Add(this.cpuGraph2);
            this.Controls.Add(this.cpuGraph1);
            this.Controls.Add(this.statusControl1);
            this.Font = new System.Drawing.Font("Unispace", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MainForm";
            this.TransparencyKey = System.Drawing.Color.Chartreuse;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public GUI.StatusControl statusControl1;
        private System.Windows.Forms.Timer timer1;
        public GUI.CPUGraph cpuGraph1;
        public GUI.CPUGraph cpuGraph2;
        public GUI.CPUGraph cpuGraph3;
        public GUI.CPUGraph cpuGraph4;
        public GUI.ThreadsControl threadsControl1;
        public GUI.NetworkGraph networkGraph1;
        public GUI.PingControl pingControl1;
        public GUI.PairSummaryControl pairSummaryControl1;
        public GUI.PairSummaryControl pairSummaryControl2;
        public GUI.PairSummaryControl pairSummaryControl3;
        public GUI.PairSummaryControl pairSummaryControl4;
        public GUI.PairSummaryControl pairSummaryControl5;
        public GUI.PairSummaryControl pairSummaryControl6;
        public GUI.PairSummaryControl pairSummaryControl7;
        public GUI.PairSummaryControl pairSummaryControl8;
        public GUI.PairSummaryControl pairSummaryControl9;
        public GUI.PairSummaryControl pairSummaryControl10;
        public GUI.PairSummaryControl pairSummaryControl11;
        public GUI.PairSummaryControl pairSummaryControl12;
        private System.Windows.Forms.ToolTip toolTip;
        public GUI.TradeHistoryControl tradeHistoryControl1;
        public GUI.ApiCallsControl apiCallsControl1;
        public GUI.MemoryControl memoryControl1;
        public GUI.ConsoleControl consoleControl1;
        public GUI.WalletControl walletControl1;
        public GUI.MainSummaryGraph mainSummaryGraph1;
        public GUI.StrategyControl strategyControl1;
        private GUI.Templates.BaseButton baseButton1;
        private GUI.Templates.BaseButton baseButton2;
        private GUI.Templates.BaseButton baseButton3;
        private GUI.Templates.BaseButton baseButton4;
        private GUI.Templates.BaseButton baseButton5;
        private System.Windows.Forms.TextBox tbConsoleInput;
        private System.Windows.Forms.Label lblConsoleInput;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem bringToFrontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utility;

namespace PoloniexBot {
    public partial class MainForm : Form {
        public MainForm () {
            InitializeComponent();

            GUI.GUIManager.SetMainFormReference(this);

            pairControls = new GUI.PairSummaryControl[] { 
                pairSummaryControl1, pairSummaryControl2, pairSummaryControl3, pairSummaryControl4, pairSummaryControl5, 
                pairSummaryControl6, pairSummaryControl7, pairSummaryControl8, pairSummaryControl9, pairSummaryControl10,
                pairSummaryControl11, pairSummaryControl12 };

            consoleButtons = new GUI.Templates.BaseButton[] {
                baseButton1, baseButton2, baseButton3, baseButton4, baseButton5
            };

            RepositionControls();

            // hook button events

            for (int i = 0; i < consoleButtons.Length; i++) {
                consoleButtons[i].MouseUp += ((sender, e) => { SetConsoleMessageTypes(); });
            }

            new Task(() => { Setup(); }).Start();
        }

        private void SetConsoleMessageTypes () {
            for (int i = 0; i < consoleButtons.Length; i++) {
                consoleControl1.DrawMessageTypes[i] = consoleButtons[i].ToggleState;
            }
            consoleControl1.Invalidate();
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void Form1_Load (object sender, EventArgs e) {
            notifyIcon.Visible = true;
        }

        private void Form1_FormClosing (object sender, FormClosingEventArgs e) {
            ClientManager.Shutdown();
            notifyIcon.Dispose();
        }

        private void Setup () {
            try {
                APICallTracker.Start();
                Utility.Log.Manager.Start();
                Utility.PerformanceMonitor.Start();
                TradeTracker.LoadData();
                ClientManager.Reboot();
                NetworkStatus.StartMonitoring();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }

        private void timer1_Tick (object sender, EventArgs e) {
            statusControl1.Invalidate();
            tradeHistoryControl1.Invalidate();
        }

        const int RightColumnWidth = 450;
        const int LeftColumnWidth = 425;

        const int ScreenBorderOffset = 10;

        const int MarginX = 2;
        const int MarginY = 2;

        public GUI.PairSummaryControl[] pairControls;
        GUI.Templates.BaseButton[] consoleButtons;

        public void RepositionControls () {

            int posX;
            int posY;

            this.Location = new Point(1920, 0);
            this.Size = new Size(1920, 1080);

            #region Left Column

            posX = ScreenBorderOffset;
            posY = ScreenBorderOffset;

            // Status Control

            statusControl1.Location = new Point(posX, posY);
            statusControl1.Size = new Size(LeftColumnWidth, 50);

            posY = ScreenBorderOffset + statusControl1.Size.Height + MarginY;

            // CPU Graphs

            int cpuGraphWidth = (LeftColumnWidth / 2) - (MarginX / 2);

            cpuGraph1.Location = new Point(ScreenBorderOffset, posY);
            cpuGraph1.Size = new Size(cpuGraphWidth, 104);

            posX = ScreenBorderOffset + cpuGraph1.Size.Width + MarginX;

            cpuGraph2.Location = new Point(posX, posY);
            cpuGraph2.Size = new Size(cpuGraphWidth + 1, 104);

            posY = cpuGraph1.Location.Y + cpuGraph1.Size.Height + MarginY;

            cpuGraph3.Location = new Point(ScreenBorderOffset, posY);
            cpuGraph3.Size = new Size(cpuGraphWidth, 104);

            cpuGraph4.Location = new Point(posX, posY);
            cpuGraph4.Size = new Size(cpuGraphWidth + 1, 104);

            posY = cpuGraph3.Location.Y + cpuGraph3.Size.Height + MarginY;

            // Memory Use Graph

            memoryControl1.Location = new Point(ScreenBorderOffset, posY);
            memoryControl1.Size = new System.Drawing.Size(LeftColumnWidth, 150);

            posY = memoryControl1.Location.Y + memoryControl1.Size.Height + MarginY;

            // Threads Indicator

            threadsControl1.Location = new Point(ScreenBorderOffset, posY);
            threadsControl1.Size = new Size(LeftColumnWidth, 286);

            posY = threadsControl1.Location.Y + threadsControl1.Size.Height + MarginY;

            // Network Graph

            networkGraph1.Location = new Point(ScreenBorderOffset, posY);
            networkGraph1.Size = new Size(LeftColumnWidth, 110);

            posY = networkGraph1.Location.Y + networkGraph1.Size.Height + MarginY;

            // Ping Graph

            pingControl1.Location = new Point(ScreenBorderOffset, posY);
            pingControl1.Size = new Size(LeftColumnWidth, 110);

            posY = pingControl1.Location.Y + pingControl1.Size.Height + MarginY;

            // Api Calls Graph

            apiCallsControl1.Location = new Point(ScreenBorderOffset, posY);
            apiCallsControl1.Size = new Size(LeftColumnWidth, Height - apiCallsControl1.Location.Y - MarginY - ScreenBorderOffset - 4);

            posY = apiCallsControl1.Location.Y + apiCallsControl1.Size.Height + MarginY;

            #endregion

            #region Center Column

            posX = ScreenBorderOffset + LeftColumnWidth + MarginX;
            posY = ScreenBorderOffset;

            // Trade History

            tradeHistoryControl1.Location = new Point(posX, posY);
            tradeHistoryControl1.Size = new Size(Width - ScreenBorderOffset - RightColumnWidth - posX - MarginX, memoryControl1.Location.Y - MarginY - ScreenBorderOffset);

            posY = tradeHistoryControl1.Location.Y + tradeHistoryControl1.Size.Height + MarginY;

            // Wallet

            walletControl1.Location = new Point(posX, posY);
            walletControl1.Size = new System.Drawing.Size(305, 262);

            // Main Graph

            mainSummaryGraph1.Location = new Point(posX + walletControl1.Size.Width + MarginX, posY);
            mainSummaryGraph1.Size = new System.Drawing.Size(Width - mainSummaryGraph1.Location.X - RightColumnWidth - MarginX - ScreenBorderOffset, 262);

            posY = mainSummaryGraph1.Location.Y + mainSummaryGraph1.Size.Height + MarginY;

            // Strategy Screens

            strategyControl1.Location = new Point(posX, posY);
            strategyControl1.Size = new System.Drawing.Size(tradeHistoryControl1.Size.Width, networkGraph1.Location.Y - strategyControl1.Location.Y - MarginY);

            posY = strategyControl1.Location.Y + strategyControl1.Size.Height + MarginY;

            // Console

            consoleControl1.Location = new Point(posX, networkGraph1.Location.Y);
            consoleControl1.Size = new Size(Width - ScreenBorderOffset - RightColumnWidth - posX - MarginX, Height - networkGraph1.Location.Y - MarginY - ScreenBorderOffset - 4);

            // Console Buttons

            posX = consoleControl1.Location.X + 70;
            posY = consoleControl1.Location.Y + 5;

            for (int i = 0; i < consoleButtons.Length; i++) {
                consoleButtons[i].Location = new Point(posX, posY);
                consoleButtons[i].Size = new Size(60, 20);

                posX += 65;
            }

            // Console Input

            posX = consoleControl1.Location.X + 5;
            posY = consoleControl1.Location.Y + consoleControl1.Size.Height - tbConsoleInput.Size.Height - 3;

            consoleControl1.SetTBInput(tbConsoleInput);

            lblConsoleInput.Location = new Point(posX, posY);

            lblConsoleInput.Font = GUI.Style.Fonts.Reduced;
            lblConsoleInput.ForeColor = GUI.Style.Colors.Primary.Main;

            tbConsoleInput.Location = new Point(posX + lblConsoleInput.Size.Width, posY);
            tbConsoleInput.Size = new Size(consoleControl1.Size.Width - lblConsoleInput.Size.Width - 10, tbConsoleInput.Size.Height);

            #endregion

            #region Right Column

            // Pair Controls
            if (pairControls != null) {

                posX = Width - ScreenBorderOffset - RightColumnWidth;
                posY = ScreenBorderOffset;

                int sizeY = (Height - (2 * ScreenBorderOffset) - (MarginY * (pairControls.Length + 1))) / pairControls.Length;

                for (int i = 0; i < pairControls.Length; i++) {
                    pairControls[i].Location = new Point(posX, posY);
                    pairControls[i].Size = new System.Drawing.Size(RightColumnWidth, sizeY);

                    posY += sizeY + MarginY;
                }
            }

            #endregion

            Invalidate();

        }

        public Control GetConsoleInputControl () {
            return tbConsoleInput;
        }

        private void bringToFrontToolStripMenuItem_Click (object sender, EventArgs e) {
            this.Focus();
        }

        private void exitToolStripMenuItem_Click (object sender, EventArgs e) {
            ThreadManager.KillAll();
            Application.Exit();
        }

        void consoleControl1_MouseWheel (object sender, System.Windows.Forms.MouseEventArgs e) {
            int scrollAmount = e.Delta;
            if (scrollAmount > 1) scrollAmount = 1;
            else if (scrollAmount < -1) scrollAmount = -1;
            else scrollAmount = 0;

            consoleControl1.ScrollMessages(scrollAmount);
            consoleControl1.Invalidate();
        }

        private void tbConsoleInput_KeyDown (object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {

                string input = tbConsoleInput.Text;
                tbConsoleInput.Text = "";

                CLI.Manager.ProcessInput(input);
            }
            if (e.KeyCode == Keys.Up) {
                tbConsoleInput.Text = CLI.Manager.GetCommandUp();
            }
            if (e.KeyCode == Keys.Down) {
                tbConsoleInput.Text = CLI.Manager.GetCommandDown();
            }
        }

    }
}

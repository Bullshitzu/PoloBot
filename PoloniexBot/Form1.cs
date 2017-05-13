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
    public partial class Form1 : Form {
        public Form1 () {
            InitializeComponent();
            Setup();
        }

        private void Form1_Load (object sender, EventArgs e) {
            notifyIcon.Visible = true;
            this.Hide();
        }

        private void Form1_FormClosing (object sender, FormClosingEventArgs e) {
            ClientManager.Shutdown();
            notifyIcon.Dispose();
        }

        private void btnExit_Click (object sender, EventArgs e) {
            ThreadManager.KillAll();
            Application.Exit();
        }

        private void Setup () {
            try {
                APICallTracker.Start();
                Utility.Log.Manager.Start();
                Windows.GUIManager.Setup();
                Windows.GUIManager.RestartThreads();
                TradeTracker.LoadData();
                ClientManager.Reboot();
                NetworkStatus.StartMonitoring();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }

        private void btnSavePositions_Click (object sender, EventArgs e) {
            Windows.GUIManager.SavePositions();
        }
    }
}

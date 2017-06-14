using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace PoloniexBot.Windows.Controls {
    public partial class PerformanceScreen : MultiThreadControl {
        public PerformanceScreen () {
            InitializeComponent();
        }

        long memoryUseLag = 0;
        int threadScroll = 0;

        Font fontTitle = new System.Drawing.Font(
                "Calibri Bold Caps", 16F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point,
                ((byte)(238)));
        Brush brush = new SolidBrush(Color.Gray);
        Brush brushEmphasis = new SolidBrush(Color.Silver);
        Brush brushUP = new SolidBrush(Color.Green);
        Brush brushDOWN = new SolidBrush(Color.Red);

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Performance";

            g.Clear(BackColor);

            g.DrawString("Performance", fontTitle, brushEmphasis, 10, 10);

            float posX = 15;
            float posY = 50;

            Process process = Process.GetCurrentProcess();

            // -----------------------------------------
            #region System

            g.DrawString("System", Font, brush, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            // todo: this
            g.DrawString("CPU Usage:", Font, brush, posX, posY);
            posX += g.MeasureString("CPU Usage:", Font).Width;
            g.DrawString("NaN %", Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            long memoryBytes = process.PrivateMemorySize64;
            memoryUseLag = Utility.Math.Lerp(memoryUseLag, memoryBytes, 0.2f);
            long memoryMb = memoryUseLag / 1000000;
            
            g.DrawString("Memory Usage:", Font, brush, posX, posY);
            posX += g.MeasureString("Memory Usage:", Font).Width;
            g.DrawString(memoryMb + " mb", Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            int threadCount = process.Threads.Count;
            g.DrawString("Active Threads:", Font, brush, posX, posY);
            posX += g.MeasureString("Active Threads:", Font).Width;
            g.DrawString(threadCount.ToString(), Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            #endregion
            // -----------------------------------------
            #region Network

            posY += 15;
            
            g.DrawString("Network", Font, brush, posX, posY);
            
            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            g.DrawString("Ping:", Font, brush, posX, posY);
            posX += g.MeasureString("Ping:", Font).Width;
            g.DrawString(Utility.NetworkStatus.lastReplyTime + " ms", Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            g.DrawString("API Calls:", Font, brush, posX, posY);
            posX += g.MeasureString("API Calls:", Font).Width;
            g.DrawString(Utility.APICallTracker.callsPerSec.ToString("F2") + " /s", Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            g.DrawString("Sent:", Font, brush, posX, posY);
            posX += g.MeasureString("Sent:", Font).Width;
            g.DrawString("NaN B/s", Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            g.DrawString("Received:", Font, brush, posX, posY);
            posX += g.MeasureString("Received:", Font).Width;
            g.DrawString("NaN B/s", Font, brushEmphasis, posX, posY);

            posX = 15;
            posY += Font.Height * 1.2f;

            // -----------------------------------------
            #endregion
            // -----------------------------------------
            #region Threads

            posX = 220;
            posY = 50;

            // -----------------------------------------

            g.DrawString("Threads", Font, brush, posX, posY);

            posX = 220;
            posY += Font.Height * 1.2f;

            // -----------------------------------------

            List<Utility.ThreadManager.ThreadData> threadData = new List<Utility.ThreadManager.ThreadData>(Utility.ThreadManager.Threads.Values.ToArray());
            threadData.Sort();

            if (threadData == null) return;
            if (threadScroll < 0) threadScroll = 0;
            if (threadScroll >= threadData.Count - 2) threadScroll = threadData.Count - 3;

            int lastShowIndex = threadScroll + 9;

            long currTime = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
            for (int i = threadScroll; i < threadData.Count && i < lastShowIndex; i++) {


                // show only 9

                g.DrawString("- " + threadData[i].name, Font, brush, posX, posY);
                posX = 420;

                long reportTime = currTime - threadData[i].lastReportTimestamp;

                string statusText;
                Brush statusBrush;

                if (threadData[i].isDone) {
                    statusText = "DONE";
                    statusBrush = brush;
                }
                else if (reportTime < 60) {
                    statusText = "UP";
                    statusBrush = brushUP;
                }
                else {
                    statusText = "DOWN";
                    statusBrush = brushDOWN;
                    Utility.NetworkStatus.BootDown();
                }

                g.DrawString(statusText, Font, statusBrush, posX, posY);

                posX = 220;
                posY += Font.Height * 1.2f;

            }

            if (threadScroll > 0) {
                g.DrawString("↑", Font, brush, 330, 50);
            }
            if (lastShowIndex < threadData.Count) {
                g.DrawString("↓", Font, brush, 330, 285);
            }

            #endregion

        }

        void PerformanceScreen_MouseWheel (object sender, MouseEventArgs e) {
            if (e.Delta == 0) return;

            if (e.Delta < 0) threadScroll++;
            else threadScroll--;
        }
    }
}

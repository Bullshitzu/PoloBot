using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.NetworkInformation;
using PoloniexBot;
using PoloniexBot.Windows;

namespace Utility {
    class NetworkStatus {

        static Thread thread;

        public static void StartMonitoring () {
            thread = ThreadManager.Register(Run, "Network Monitor", false);
        }

        public static void Stop () {
            ThreadManager.Kill(thread);
        }

        static bool netDown = false;
        static int missCount = 0;

        public static long lastReplyTime = 0;

        static void Run () {
            while (true) {
                try {
                    Ping ping = new Ping();
                    PingReply reply = ping.Send("8.8.8.8");

                    if (reply.Status == IPStatus.Success) {
                        lastReplyTime = reply.RoundtripTime;
                        missCount = 0;
                        if (netDown) {
                            GUIManager.errorWindow.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                                GUIManager.errorWindow.ShowResolved("Network Restored");
                            });
                            netDown = false;
                            ErrorLog.ReportLog("Network Restored");
                            ClientManager.Reboot();
                        }
                    }
                    else if (!netDown) {
                        missCount++;
                        if (missCount > 3) {
                            // netDown = true;
                            ErrorLog.ReportError("Network Down");
                            // BootDown();
                        }
                    }
                }
                catch (PingException) {
                    missCount++;
                    if (missCount > 3) {
                        if (!netDown) {
                            // netDown = true;
                            ErrorLog.ReportError("Network Down");
                            // BootDown();
                        }
                    }
                }

                ThreadManager.ReportAlive("NetworkStatus");
                Thread.Sleep(1000);
            }
        }

        public static void BootDown () {

            ClientManager.Shutdown();
            netDown = true;

            GUIManager.errorWindow.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                GUIManager.errorWindow.ShowError("Network Down");
            });
        }
    }
}

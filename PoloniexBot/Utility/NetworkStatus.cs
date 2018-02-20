using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.NetworkInformation;
using PoloniexBot;

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

                        PoloniexBot.GUI.GUIManager.UpdatePing(lastReplyTime);

                        missCount = 0;
                        if (netDown) {
                            PoloniexBot.GUI.GUIManager.SetNetworkStateMessage(PoloniexBot.GUI.MainSummaryGraph.NetworkMessageState.Restored);
                            netDown = false;

                            new Task(() => {
                                Utility.Log.Manager.LogBasicMessage("REBOOT");
                                ClientManager.Reboot();
                            }).Start();

                            ErrorLog.ReportLog("Network Restored");
                        }
                    }
                    else if (!netDown) {
                        missCount++;

                        PoloniexBot.GUI.GUIManager.UpdatePing(0);

                        if (missCount > 3) {
                            netDown = true;
                            PoloniexBot.GUI.GUIManager.SetNetworkStateMessage(PoloniexBot.GUI.MainSummaryGraph.NetworkMessageState.Down);
                            ErrorLog.ReportError("Network Down");
                        }
                    }
                }
                catch (PingException) {
                    missCount++;

                    PoloniexBot.GUI.GUIManager.UpdatePing(0);

                    if (missCount > 3) {
                        if (!netDown) {
                            netDown = true;
                            PoloniexBot.GUI.GUIManager.SetNetworkStateMessage(PoloniexBot.GUI.MainSummaryGraph.NetworkMessageState.Down);
                            ErrorLog.ReportError("Network Down");
                        }
                    }
                }

                ThreadManager.ReportAlive("NetworkStatus");
                Thread.Sleep(1000);
            }
        }
    }
}

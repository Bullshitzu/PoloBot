using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Utility {
    public static class APICallTracker {

        static TSList<float> calls;
        static Thread thread;

        public static float callsPerSec = 0;

        public static void Start () {
            calls = new TSList<float>();
            thread = ThreadManager.Register(Run, "API Call Tracker", false);
        }
        public static void Stop () {
            ThreadManager.Kill(thread);
        }

        public static void ReportApiCall () {
            double sum = 1;
            int cnt = 1;

            for (int i = calls.Count-1; i >= 0; i--) {
                sum += calls[i];
                cnt++;
            }

            calls.Add((float)(sum / cnt));
        }

        static void Run () {
            while (true) {

                lock (calls) {
                    for (int i = 0; i < calls.Count; i++) {
                        calls[i] -= 0.1f;
                    }
                }
                
                calls.RemoveAll(HasExpired);

                if (calls.Count > 0) callsPerSec = calls.Last();
                else callsPerSec = 0;

                PoloniexBot.GUI.GUIManager.UpdateApiCalls(callsPerSec);

                ThreadManager.ReportAlive("APICallTracker");
                Thread.Sleep(1000);
            }
        }

        static bool HasExpired (float timer) {
            return timer <= 0;
        }
    }
}

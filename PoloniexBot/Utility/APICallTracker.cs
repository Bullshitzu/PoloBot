using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PoloniexBot.Windows;

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
            calls.Add(1);
        }

        static void Run () {
            while (true) {

                float sum = 0;
                lock (calls) {
                    for (int i = 0; i < calls.Count; i++) {
                        sum += calls[i];
                        calls[i] -= 0.001f;
                    }
                }
                
                calls.RemoveAll(HasExpired);
                callsPerSec = sum / 10f;

                ThreadManager.ReportAlive();
                Thread.Sleep(10);
            }
        }

        static bool HasExpired (float timer) {
            return timer <= 0;
        }
    }
}

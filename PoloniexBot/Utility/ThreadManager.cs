using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Utility {
    public static class ThreadManager {

        public class ThreadData : IEquatable<Thread>, IComparable<ThreadData> {
            public Thread thread;
            public SimpleMethod method;
            public bool isNetworkSensitive;
            public string name;
            public long lastReportTimestamp;
            public int sortPriority;
            public bool isDone = false;

            public ThreadData (SimpleMethod method, string name, bool isNetworkSensitive) {
                this.thread = new Thread(new ThreadStart(ThreadLoop));
                this.method = method;
                this.name = name;
                this.isNetworkSensitive = isNetworkSensitive;
            }

            public bool Equals (Thread other) {
                return this.thread == other;
            }

            private void ThreadLoop () {
                while (true) {
                    ReportAlive(name);

                    try {
                        method();
                        if(Threads!=null) Threads.Remove(thread);
                        return;
                    }
                    catch (ThreadAbortException) {
                        if (Threads != null) Threads.Remove(thread);
                        return;
                    }
                    catch (Exception e) {
                        PoloniexBot.CLI.Manager.PrintError("Error in thread \"" + name + "\": " + e.Message);
                        Console.WriteLine("Error cought in thread " + name + ": " + e.Message + "\n" + e.StackTrace);
                        Console.WriteLine("Trying again!");
                    }
                }
            }

            public int CompareTo (ThreadData other) {
                return this.name.CompareTo(other.name) + (this.sortPriority.CompareTo(other.sortPriority)) * 5;
            }
        }
        public static TSDictionary<Thread, ThreadData> Threads;
        public delegate void SimpleMethod ();

        static ThreadManager () {
            Threads = new TSDictionary<Thread, ThreadData>();
        }

        public static Thread Register (SimpleMethod method, string threadName, bool networkSensitive) {
            return Register(method, threadName, networkSensitive, 0);
        }
        public static Thread Register (SimpleMethod method, string threadName, bool networkSensitive, int sortPriority) {
            ThreadData td = new ThreadData(method, threadName, networkSensitive);
            td.sortPriority = sortPriority;

            ThreadData[] currThreads = Threads.Values.ToArray();
            for (int i = 0; i < currThreads.Length; i++) {
                if (currThreads[i].name == threadName) {
                    Threads.Remove(currThreads[i].thread);
                    break;
                }
            }

            Threads.Add(td.thread, td);

            td.thread.Start();
            return td.thread;
        }

        public static void ReportAlive (string name) {
            ThreadData td;
            if (Threads.TryGetValue(Thread.CurrentThread, out td)) {
                td.name = name;
                td.lastReportTimestamp = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
            }
            else {
                Console.WriteLine("Cannot report thread " + name + " - not registered!");
            }
        }

        public static void Wait (Thread t) {
            if (t == null) return;
            if (Threads.ContainsKey(t)) {
                if (t.ThreadState != ThreadState.Unstarted) {
                    t.Join();
                    Threads.Remove(t);
                }
            }
        }
        public static void Kill (Thread t) {
            if (t == null) return;
            if (Threads.ContainsKey(t)) {
                if (t.ThreadState != ThreadState.Unstarted) {
                    if (t.IsAlive) t.Abort();
                    t.Join();
                }
            }
        }

        public static void KillNetwork () {
            ThreadData[] data = Threads.ValuesToArray();
            if (data != null) {
                for (int i = 0; i < data.Length; i++) {
                    if (data[i].isNetworkSensitive) Kill(data[i].thread);
                }
            }
        }
        public static void KillAll () {
            KillNetwork();
            ThreadData[] data = Threads.ValuesToArray();
            if (data != null) {
                for (int i = 0; i < data.Length; i++) {
                    Kill(data[i].thread);
                }
            }
        }

        public static int GetThreadCount (bool networkSensitive) {
            int cnt = 0;
            ThreadData[] data = Threads.ValuesToArray();
            if (data != null) {
                for (int i = 0; i < data.Length; i++) {
                    if (data[i].isNetworkSensitive == networkSensitive) cnt++;
                }
            }
            if (!networkSensitive) cnt++; // to account for the main thread, which is not tracked here
            return cnt;
        }
    }
}

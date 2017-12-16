using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using PoloniexBot.GUI;
using System.Net.NetworkInformation;

namespace Utility {
    class PerformanceMonitor {

        private static System.Threading.Thread thread;

        public static void Start () {
            thread = ThreadManager.Register(Run, "Peformance Monitor", false);
        }
        public static void Stop () {
            ThreadManager.Kill(thread);
        }

        private const int sleepPeriod = 1000;

        private static void Run () {

            PerformanceCounter CPUCounter1 = new PerformanceCounter("Processor", "% Processor Time", "0");
            PerformanceCounter CPUCounter2 = new PerformanceCounter("Processor", "% Processor Time", "1");
            PerformanceCounter CPUCounter3 = new PerformanceCounter("Processor", "% Processor Time", "2");
            PerformanceCounter CPUCounter4 = new PerformanceCounter("Processor", "% Processor Time", "3");

            PerformanceCounter memoryCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);

            NetworkInterface netInterface = NetworkInterface.GetAllNetworkInterfaces().First();

            long lastBytesReceived = netInterface.GetIPv4Statistics().BytesReceived;
            long lastBytesSent = netInterface.GetIPv4Statistics().BytesSent;

            while (true) {
                System.Threading.Thread.Sleep(sleepPeriod);
                try {

                    // cpu
                    GUIManager.UpdateCPU1(CPUCounter1.NextValue());
                    GUIManager.UpdateCPU2(CPUCounter2.NextValue());
                    GUIManager.UpdateCPU3(CPUCounter3.NextValue());
                    GUIManager.UpdateCPU4(CPUCounter4.NextValue());

                    // memory
                    GUIManager.UpdateMemory(memoryCounter.NextValue());

                    // network
                    long bytesReceived = netInterface.GetIPv4Statistics().BytesReceived;
                    long bytesSent = netInterface.GetIPv4Statistics().BytesSent;

                    GUIManager.UpdateNetwork(bytesSent - lastBytesSent, bytesReceived - lastBytesReceived);

                    lastBytesReceived = bytesReceived;
                    lastBytesSent = bytesSent;

                    // thread count
                    int threadCount = Process.GetCurrentProcess().Threads.Count;
                    GUIManager.UpdateActiveThreadCount(threadCount);

                }
                catch (System.Threading.ThreadAbortException) {
                    // this is normal, shutdown was called
                    return;
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message + " - " + e.StackTrace);
                }
            }
        }




    }
}

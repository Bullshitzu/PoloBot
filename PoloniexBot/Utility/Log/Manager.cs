using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using PoloniexAPI;

namespace Utility.Log {
    public static class Manager {

        const string FolderName = "Logs";

        const string FilenameNetLog = "Net.log";
        const string FilenameErrorLog = "Error.log";

        static TSList<MessageTypes.NetMessage> NetMessages;
        static TSList<MessageTypes.ErrorMessage> ErrorMessages;

        static Thread thread;

        public static void Start () {
            NetMessages = new TSList<MessageTypes.NetMessage>();
            ErrorMessages = new TSList<MessageTypes.ErrorMessage>();
            thread = ThreadManager.Register(Run, "Log", false);
        }
        public static void Stop () {
            ThreadManager.Kill(thread);
        }

        static void Run () {
            if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);

            ErrorMessages.Clear();
            NetMessages.Clear();

            return;
            // todo: enable logging?
            // note: eats HDD memory like mad

            while (true) {

                ResolveNetLogs();
                ResolveErrorLogs();

                ThreadManager.ReportAlive("Log.Manager");
                Thread.Sleep(10);
            }
        }

        // ----------------------------------------

        public static void LogNetSent (string message) {
            NetMessages.Add(new MessageTypes.NetMessage(true, message));
        }
        public static void LogNetReceived (string message) {
            NetMessages.Add(new MessageTypes.NetMessage(false, message));
        }
        static void ResolveNetLogs () {
            if (NetMessages.Count == 0) return;

            List<string> lines = new List<string>();
            while (NetMessages.Count > 0) {

                lines.Add(NetMessages[0].ToString());
                lines.Add("");
                lines.Add("");

                NetMessages.RemoveAt(0);
            }

            FileManager.SaveFileConcat(FolderName + "/" + FilenameNetLog, lines.ToArray());
        }

        // ----------------------------------------

        public static void LogError (string message) {
            ErrorMessages.Add(new MessageTypes.ErrorMessage(message));
        }
        public static void LogError (string message, string stackTrace) {
            ErrorMessages.Add(new MessageTypes.ErrorMessage(message, stackTrace));
        }
        static void ResolveErrorLogs () {
            if (ErrorMessages.Count == 0) return;

            List<string> lines = new List<string>();
            while (ErrorMessages.Count > 0) {

                lines.Add(ErrorMessages[0].ToString());
                lines.Add("");
                lines.Add("");

                ErrorMessages.RemoveAt(0);
            }

            FileManager.SaveFileConcat(FolderName + "/" + FilenameErrorLog, lines.ToArray());
        }


    }
}

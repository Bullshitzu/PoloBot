using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexBot.CLI;

namespace Utility {
    public static class ErrorLog {

        public static void ReportError (string message) {
            Utility.Log.Manager.LogError(message);
            Manager.PrintError(message);
        }
        public static void ReportError (Exception e) {
            Utility.Log.Manager.LogError(e.Message, e.StackTrace);
            Manager.PrintError(e.Message);
        }
        public static void ReportError (string message, Exception e) {
            Utility.Log.Manager.LogError(message + " - " + e.Message, e.StackTrace);
            Manager.PrintError(message);
        }
        public static void ReportLog (string message) {
            Manager.PrintLog(message);
        }

        public static void ReportErrorSilent (string message) {
            Utility.Log.Manager.LogError(message);
        }
        public static void ReportErrorSilent (Exception e) {
            Utility.Log.Manager.LogError(e.Message, e.StackTrace);
        }
        public static void ReportErrorSilent (string message, Exception e) {
            Utility.Log.Manager.LogError(message + " - " + e.Message, e.StackTrace);
        }
        public static void ReportLogSilent (string message) {
            Manager.PrintLog(message);
        }
    }
}

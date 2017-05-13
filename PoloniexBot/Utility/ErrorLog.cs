using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexBot.CLI;

namespace Utility {
    public static class ErrorLog {

        public static void ReportError (string message) {
            Manager.PrintError(message);
        }
        public static void ReportError (Exception e) {
            Manager.PrintError(e.Message);
        }
        public static void ReportError (string message, Exception e) {
            Manager.PrintError(message);
        }
        public static void ReportLog (string message) {
            Manager.PrintLog(message);
        }

        public static void ReportErrorSilent (string message) {
            Manager.PrintError(message);
        }
        public static void ReportErrorSilent (Exception e) {
            Manager.PrintError(e.Message);
        }
        public static void ReportErrorSilent (string message, Exception e) {
            Manager.PrintError(message);
        }
        public static void ReportLogSilent (string message) {
            Manager.PrintLog(message);
        }



        // todo: also log these somewhere
        // in an actual file.....

    }
}

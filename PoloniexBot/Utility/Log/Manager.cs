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
        const string FilenameGenericLog = "Base.log";

        static TSList<MessageTypes.Message> BasicMessages;
        static TSList<MessageTypes.NetMessage> NetMessages;
        static TSList<MessageTypes.ErrorMessage> ErrorMessages;

        public static void Initialize () {
            if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);
            NetMessages = new TSList<MessageTypes.NetMessage>();
            ErrorMessages = new TSList<MessageTypes.ErrorMessage>();
            BasicMessages = new TSList<MessageTypes.Message>();
        }

        // ----------------------------------------

        public static void LogBasicMessage (string message) {
            BasicMessages.Add(new MessageTypes.Message(message));
            ClearOldMessages();
            SaveMessagesToFiles();
            UpdateGUI();
        }
        private static void ClearOldMessages () {
            if (BasicMessages == null) return;

            long clearTime = PoloniexBot.GUI.GUIManager.GetTradeHistoryEndTime();
            BasicMessages.RemoveAll(x => Utility.DateTimeHelper.DateTimeToUnixTimestamp(x.Time) < clearTime);
        }

        // ----------------------------------------

        public static void LogNetSent (string message) {
            // NetMessages.Add(new MessageTypes.NetMessage(true, message));
        }
        public static void LogNetReceived (string message) {
            // NetMessages.Add(new MessageTypes.NetMessage(false, message));
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
            ClearOldErrorMessages();
            SaveMessagesToFiles();
            UpdateGUI();
        }
        public static void LogError (string message, string stackTrace) {
            ErrorMessages.Add(new MessageTypes.ErrorMessage(message, stackTrace));
            ClearOldErrorMessages();
            SaveMessagesToFiles();
            UpdateGUI();
        }
        private static void ClearOldErrorMessages () {
            if (ErrorMessages == null) return;
            
            long clearTime = PoloniexBot.GUI.GUIManager.GetTradeHistoryEndTime();
            ErrorMessages.RemoveAll(x => Utility.DateTimeHelper.DateTimeToUnixTimestamp(x.Time) < clearTime);
        }
        
        // ----------------------------------------

        public static void UpdateGUI () {
            PoloniexBot.GUI.GUIManager.SetTradeHistoryMessages(BasicMessages, ErrorMessages);
        }

        public static void LoadMessagesFromFiles () {
            if (BasicMessages == null) BasicMessages = new TSList<MessageTypes.Message>();
            if (ErrorMessages == null) ErrorMessages = new TSList<MessageTypes.ErrorMessage>();

            BasicMessages.Clear();
            ErrorMessages.Clear();

            string filePathBasic = FolderName + "/" + FilenameGenericLog;
            string filePathError = FolderName + "/" + FilenameErrorLog;

            // load basic messages
            try {
                string[] lines = Utility.FileManager.ReadFile(filePathBasic);
                for (int i = 0; i < lines.Length; i++) {
                    MessageTypes.Message m = MessageTypes.Message.Parse(lines[i]);
                    BasicMessages.Add(m);
                }
            }
            catch (Exception) { }

            // load error messages
            try {
                string[] lines = Utility.FileManager.ReadFile(filePathError);
                for (int i = 0; i < lines.Length; i++) {
                    MessageTypes.ErrorMessage m = MessageTypes.ErrorMessage.Parse(lines[i]);
                    ErrorMessages.Add(m);
                }
            }
            catch (Exception) { }

            PoloniexBot.GUI.GUIManager.SetTradeHistoryMessages(BasicMessages, ErrorMessages);
        }
        public static void SaveMessagesToFiles () {

            // save basic messages
            List<string> lines = new List<string>();
            for (int i = 0; i < BasicMessages.Count; i++) {
                lines.Add(BasicMessages[i].ToStringFile());
            }

            FileManager.SaveFile(FolderName + "/" + FilenameGenericLog, lines.ToArray());

            // save error messages
            lines = new List<string>();
            for (int i = 0; i < ErrorMessages.Count; i++) {
                lines.Add(ErrorMessages[i].ToStringFile());
            }

            FileManager.SaveFile(FolderName + "/" + FilenameErrorLog, lines.ToArray());
        }
    }
}

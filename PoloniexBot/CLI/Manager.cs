using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexBot.CLI.Commands;

namespace PoloniexBot.CLI {
    public static class Manager {

        public enum MessageType {
            User,
            Note,
            Log,
            Warning,
            Error,
            NoHeader,
        }
        public struct Message {
            public DateTime date;
            public MessageType type;
            public string message;

            public Message (MessageType type, string message) {
                this.type = type;
                this.message = message;
                date = DateTime.Now;
            }
        }

        static List<Message> messages;
        internal static Command[] commands;
        
        static List<string> commandHistory;
        static int commandHistoryIndex = 0;

        static Manager () {
            messages = new List<Message>();
            commandHistory = new List<string>();
            InitializeCommands();
        }
        static void InitializeCommands () {
            List<Command> comms = new List<Command>();

            comms.Add(new Command("clear", "Clears the CLI", false, CommandImplementations.Clear));
            comms.Add(new Command("refresh", "Refreshes the trading pair list", CommandImplementations.Refresh));
            
            comms.Add(new Command("block", "Cancels orders and stops trading pair(s)", true, new Parameter[] { new Parameter("pair/all") }, CommandImplementations.Block));
            comms.Add(new Command("resume", "Resumes trading pair(s)", true, new Parameter[] { new Parameter("pair/all") }, CommandImplementations.Resume));
            
            comms.Add(new Command("help", "Shows this help menu", CommandImplementations.Help));

            comms.Add(new Command("savetradedata", "Saves trade data to file", CommandImplementations.SaveTradeData));
            comms.Add(new Command("loadtradedata", "Loads trade data from file", CommandImplementations.LoadTradeData));

            comms.Add(new Command("savepatterns", "Saves pattern data to file", CommandImplementations.SavePatternData));
            comms.Add(new Command("loadpatterns", "Loads pattern data from file", CommandImplementations.LoadPatternData));

            comms.Add(new Command("buy", "Forces a manual buy of the specified trade pair", true, new Parameter[] { new Parameter("currency") }, CommandImplementations.ForceBuy));
            comms.Add(new Command("sell", "Forces a manual sell of the specified trade pair", true, new Parameter[] { new Parameter("currency") }, CommandImplementations.ForceSell));

            comms.Add(new Command("simulate", "Runs a simulation on stored ticker data", new Parameter[] { new Parameter("ideal") }, CommandImplementations.Simulate));

            comms.Add(new Command("mark", "Marks a specified trade pair for display", true, new Parameter[] { new Parameter("currency") }, CommandImplementations.MarkPair));
            comms.Add(new Command("unmark", "Unmarks a specified trade pair for display", true, new Parameter[] { new Parameter("currency") }, CommandImplementations.UnmarkPair));

            // Fun stuff

            comms.Add(new Command("printdragon", "sexy", CommandImplementations.PrintDragon));

            commands = new Command[comms.Count];
            for (int i = 0; i < comms.Count; i++) {
                commands[i] = comms[i];
            }
        }

        public static void PrintNote (string text) {
            messages.Insert(0, new Message(MessageType.Note, text));
            RefreshWindow();
        }
        public static void PrintWarning (string text) {
            messages.Insert(0, new Message(MessageType.Warning, text));
            RefreshWindow();
        }
        public static void PrintError (string text) {
            messages.Insert(0, new Message(MessageType.Error, text));
            RefreshWindow();
        }
        public static void PrintLog (string text) {
            messages.Insert(0, new Message(MessageType.Log, text));
            RefreshWindow();
        }

        internal static void PrintNoHeader (string text) {
            messages.Insert(0, new Message(MessageType.NoHeader, text));
            RefreshWindow();
        }

        static void RefreshWindow () {
            while (messages.Count > 200) messages.RemoveAt(messages.Count - 1);
            GUI.GUIManager.SetCLIMessages(messages.ToArray());
        }
        internal static void ClearWindow () {
            messages.Clear();
            RefreshWindow();
        }

        public static void ProcessInput (string text) {

            string cleanedInput = text.ToLower().Trim();
            string[] parts = cleanedInput.Split(' ');

            if (parts.Length == 0) {
                messages.Insert(0, new Message(MessageType.User, ""));
                RefreshWindow();
                return;
            }

            commandHistory.Add(text);
            commandHistoryIndex = commandHistory.Count;

            if (commands == null || commands.Length == 0) {
                PrintError("Commands array not initialized or is empty");
                return;
            }

            for (int i = 0; i < commands.Length; i++) {
                if (commands[i].CompareKeyword(parts[0])) {

                    if (commands[i].GetEcho()) {
                        messages.Insert(0, new Message(MessageType.User, text));
                        RefreshWindow();
                    }

                    try {
                        commands[i].Execute(parts);
                    }
                    catch (Exception e) {
                        PrintError(e.Message);
                    }
                    return;
                }
            }

            PrintError("Command \"" + parts[0] + "\" not recognized. Use \"help\" for a list of commands");
        }

        public static string GetCommandUp () {
            commandHistoryIndex--;
            if (commandHistoryIndex < 0) commandHistoryIndex = 0;
            if (commandHistoryIndex >= 0 && commandHistoryIndex < commandHistory.Count) return commandHistory[commandHistoryIndex];
            return "";
        }
        public static string GetCommandDown () {
            commandHistoryIndex++;
            if (commandHistoryIndex > commandHistory.Count) commandHistoryIndex = commandHistory.Count;
            if (commandHistoryIndex >= 0 && commandHistoryIndex < commandHistory.Count) return commandHistory[commandHistoryIndex];
            return "";
        }
    }
}

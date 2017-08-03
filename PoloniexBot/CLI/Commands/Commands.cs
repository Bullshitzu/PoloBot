using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.CLI {
    class CommandImplementations {
        public static void Help (string[] parameters) {
            for (int i = 0; i < Manager.commands.Length; i++) {
                Manager.PrintNote(Manager.commands[i].ToString());
            }
        }
        public static void Clear (string[] parameters) {
            Manager.ClearWindow();
        }
        public static void Stop (string[] parameters) {
            throw new NotImplementedException();
        }
        public static void Resume (string[] parameters) {
            throw new NotImplementedException();
        }
        public static void Cease (string[] parameters) {
            throw new NotImplementedException();
        }
        public static void Refresh (string[] parameters) {
            Utility.ThreadManager.Register(() => {
                Trading.Manager.Stop();
                Trading.Manager.RefreshTradePairs();
                Trading.Manager.Start();
            }, "Refresh Trade Pairs", true);
        }

        public static void SaveTradeData (string[] parameters) {
            Utility.ThreadManager.Register(Data.Store.SaveTradeData, "Trade Data Save", false);
        }
        public static void LoadTradeData (string[] parameters) {
            Utility.ThreadManager.Register(() => { Data.Store.LoadTradeData(); }, "Trade Data Load", false);
        }

        public static void SavePatternData (string[] parameters) {
            Utility.ThreadManager.Register(() => { Data.PatternMatching.Manager.SaveToFile(); }, "Pattern Data Save", false);
        }
        public static void LoadPatternData (string[] parameters) {
            Utility.ThreadManager.Register(() => { Data.PatternMatching.Manager.LoadFromFile(); }, "Pattern Data Load", false);
        }

        public static void ForceBuy (string[] parameters) {
            if (parameters == null || parameters.Length != 2) throw new Exception("Wrong Parameters");

            Utility.ThreadManager.Register(() => {
                PoloniexAPI.CurrencyPair pair = new PoloniexAPI.CurrencyPair("BTC", parameters[1].ToUpper());
                if (!Trading.Manager.ForceBuy(pair)) {
                    throw new Exception("Specified currency does not exist");
                }
            }, "Force Buy", true);
        }
        public static void ForceSell (string[] parameters) {
            if (parameters == null || parameters.Length != 2) throw new Exception("Wrong Parameters");

            Utility.ThreadManager.Register(() => {
                PoloniexAPI.CurrencyPair pair = new PoloniexAPI.CurrencyPair("BTC", parameters[1].ToUpper());
                if (!Trading.Manager.ForceSell(pair)) {
                    throw new Exception("Specified currency does not exist");
                }
            }, "Force Sell", true);
        }

    }
}

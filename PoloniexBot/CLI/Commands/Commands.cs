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
            throw new NotImplementedException();
        }

        public static void Simulate (string[] parameters) {
            string pairName = parameters[1].Replace('-', '_').Replace('/', '_').ToUpper();

            if (pairName == "ALL") {
                Utility.ThreadManager.Register(Simulation.SimulateAllConsecutively, "Simulator", false);
            }
            else {
                PoloniexAPI.CurrencyPair pair = PoloniexAPI.CurrencyPair.Parse(pairName);
                if (pair != null && Data.Store.CheckPairExists(pair)) {
                    Manager.PrintNote("Running simulation on pair " + pair);
                    Trading.Manager.GetTPManager(pair).InvokeThread(Trading.Manager.GetTPManager(pair).RunSimulation);
                }
                else throw new Exception("Pair " + pair + " does not exist");
            }
        }

        public static void Train (string[] parameters) {
            Utility.ThreadManager.Register(Training.Train, "Pattern-Matched Training", false);
        }

        public static void SaveTradeData (string[] parameters) {
            Utility.ThreadManager.Register(Data.Store.SaveTradeData, "Trade Data Save", false);
        }
        public static void LoadTradeData (string[] parameters) {
            Utility.ThreadManager.Register(Data.Store.LoadTradeData, "Trade Data Load", false);
        }

    }
}

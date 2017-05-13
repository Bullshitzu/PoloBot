using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot {
    static class Simulation {

        static Simulation () {
            wallet = (PoloniexAPI.WalletTools.WalletSimulated)PoloniexBot.ClientManager.client.Wallet;
        }

        static PoloniexAPI.WalletTools.WalletSimulated wallet;
        public static void ResetWallet () {
            wallet.Reset();
        }

        public static void PostOrder (CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote) {
            wallet.DoTransaction(currencyPair, type, pricePerCoin, amountQuote);
        }

        // ----------------------

        static double[] geneticVars = { 0.0001071, 2, 0.02821, 1.75, 1, 1 };
        static double[] oldVars = { 0.0001071, 2, 0.02821, 1.75, 1, 1 };
        static double mutationAmount = 0.05; // 5% mutation

        public static void SimulateAllConsecutively () {
            
            Simulation.ResetWallet();
            Utility.TradeTracker.ClearAll();

            Trading.TPManager[] managers = Trading.Manager.GetAllTPManagers();
            if (managers == null || managers.Length == 0) throw new Exception("TP Manager list is empty");
            CLI.Manager.PrintNote("Running simulation on all " + managers.Length + " pairs");
            for (int i = 0; i < managers.Length; i++) {
                managers[i].RunSimulation();
            }
            CLI.Manager.PrintNote("Done!");
        }

        public static double CalculateSimulationScore (double adx, double volumeFactor, double meanRevScore) {

            adx = (geneticVars[0] * System.Math.Pow(adx, geneticVars[1])) - (geneticVars[2] * adx) + geneticVars[3];

            volumeFactor = System.Math.Pow(volumeFactor, geneticVars[4]);
            meanRevScore = System.Math.Pow(meanRevScore, geneticVars[5]);

            return meanRevScore * adx * volumeFactor;
        }
    }
}

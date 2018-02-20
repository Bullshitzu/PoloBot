using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Utility;

namespace PoloniexBot.GUI {
    public static class GUIManager {

        private static MainForm mainForm;
        public static void SetMainFormReference (MainForm form) {
            mainForm = form;
        }

        // --------------------------

        public enum Environment {
            Development,
            Simulation,
            Live
        }
        public static Environment environment;

        public static void SetEnvironment (Environment e) {
            environment = e;
            mainForm.statusControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateCPU1 (double val) {
            mainForm.cpuGraph1.UpdateCPUValue(val);
            mainForm.cpuGraph1.Invalidate();
        }
        public static void UpdateCPU2 (double val) {
            mainForm.cpuGraph2.UpdateCPUValue(val);
            mainForm.cpuGraph2.Invalidate();
        }
        public static void UpdateCPU3 (double val) {
            mainForm.cpuGraph3.UpdateCPUValue(val);
            mainForm.cpuGraph3.Invalidate();
        }
        public static void UpdateCPU4 (double val) {
            mainForm.cpuGraph4.UpdateCPUValue(val);
            mainForm.cpuGraph4.Invalidate();
        }

        // --------------------------

        public static void UpdateMemory (double val) {
            mainForm.memoryControl1.UpdateMemoryValue(val);
            mainForm.memoryControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateModulesPairs (string[] pairs) {
            mainForm.threadsControl1.ActivePairs = pairs;
            mainForm.threadsControl1.Invalidate();
        }
        public static void UpdateActiveThreadCount (int count) {
            mainForm.threadsControl1.ActiveThreadCount = count;
            mainForm.threadsControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateNetwork (long up, long down) {
            mainForm.networkGraph1.UpdateNetworkValue(up, down);
            mainForm.networkGraph1.Invalidate();
        }

        // --------------------------

        public static void UpdatePing (long val) {
            mainForm.pingControl1.UpdatePingValue(val);
            mainForm.pingControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateApiCalls (double val) {
            mainForm.apiCallsControl1.UpdateAPICallValue(val);
            mainForm.apiCallsControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateTradeHistory (TSList<Utility.TradeTracker.TradeData> trades, TSList<Utility.TradeTracker.TradeData> doneTrades) {
            mainForm.tradeHistoryControl1.openPositions = trades;
            mainForm.tradeHistoryControl1.closedPositions = doneTrades;
            mainForm.tradeHistoryControl1.Invalidate();
        }
        public static void SetTradeHistoryEndTime (long time, bool repaint = false) {
            mainForm.tradeHistoryControl1.chartEndTime = time;
            if (repaint) mainForm.tradeHistoryControl1.Invalidate();
        }
        public static long GetTradeHistoryEndTime () {
            return mainForm.tradeHistoryControl1.chartEndTime;
        }
        public static void SetTradeHistoryMessages (TSList<Utility.Log.MessageTypes.Message> basicMessages, TSList<Utility.Log.MessageTypes.ErrorMessage> errorMessages) {
            mainForm.tradeHistoryControl1.basicMessages = basicMessages;
            mainForm.tradeHistoryControl1.errorMessages = errorMessages;
            mainForm.tradeHistoryControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateWallet (KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] balances) {

            List<KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>> balanceList = new List<KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>>(balances);
            balanceList.Sort((a, b) => a.Value.BitcoinValue.CompareTo(b.Value.BitcoinValue));
            balanceList.Reverse();

            mainForm.walletControl1.balances = balanceList.ToArray();
            mainForm.walletControl1.Invalidate();
        }

        // --------------------------

        public static void UpdateMainSummary (PoloniexAPI.TickerChangedEventArgs[] tickers) {
            if (tickers == null || tickers.Length == 0) return;
            mainForm.mainSummaryGraph1.UpdateTickers(tickers);
            mainForm.mainSummaryGraph1.Invalidate();
        }
        public static void SetNetworkStateMessage (MainSummaryGraph.NetworkMessageState state) {
            mainForm.mainSummaryGraph1.NetworkMessage = state;
            mainForm.mainSummaryGraph1.Invalidate();

            if (state == MainSummaryGraph.NetworkMessageState.Restored) {
                new Task(() => {
                    System.Threading.Thread.Sleep(10000);
                    SetNetworkStateMessage(MainSummaryGraph.NetworkMessageState.Hide);
                }).Start();
            }
        }

        public static void SetMainMarked (bool state) {
            mainForm.mainSummaryGraph1.MarkedBorder = state;
            mainForm.mainSummaryGraph1.Invalidate();
        }

        // --------------------------

        public static void AddStrategyScreenPair (PoloniexAPI.CurrencyPair pair) {
            mainForm.strategyControl1.AddPairData(pair);
            mainForm.strategyControl1.Invalidate();
        }
        public static void UpdateStrategyScreenPair (PoloniexAPI.CurrencyPair pair, Dictionary<string, double> ruleVariables) {
            mainForm.strategyControl1.UpdatePairData(pair, ruleVariables);
            mainForm.strategyControl1.Invalidate();
        }
        public static void BlockPair (PoloniexAPI.CurrencyPair pair, bool state) {
            mainForm.strategyControl1.SetBlockedPairData(pair, state);
            mainForm.strategyControl1.Invalidate();

            PairSummariesManager.SetPairBlock(pair, state);
            UpdatePairSummaries();
        }
        public static void ClearStrategyScreen () {
            mainForm.strategyControl1.ClearPairData();
            mainForm.strategyControl1.Invalidate();
        }

        // --------------------------

        public static void SetCLIMessages (PoloniexBot.CLI.Manager.Message[] messages) {
            if (messages == null) return;
            mainForm.consoleControl1.SetMessages(messages);
            mainForm.consoleControl1.Invalidate();
        }

        // --------------------------

        private static void UpdatePairSummaries () {

            List<PairSummariesManager.PairSummary> pairs = new List<PairSummariesManager.PairSummary>(PairSummariesManager.GetPairsSorted());
            
            pairs.Sort();
            pairs.Reverse();

            for (int i = 0; i < mainForm.pairControls.Length; i++) {
                mainForm.pairControls[i].SetToNoData();
                if (i < pairs.Count) {
                    mainForm.pairControls[i].UpdatePair(pairs[i].Pair, pairs[i].Tickers, pairs[i].MarkedUser);
                    mainForm.pairControls[i].SetBlocked(pairs[i].Blocked);
                }

                mainForm.pairControls[i].Invalidate();
            }
        }

        public static void SetPairSummary (PoloniexAPI.CurrencyPair pair, PoloniexAPI.TickerChangedEventArgs[] tickers, double volume) {
            PairSummariesManager.SetPairSummary(pair, tickers, volume);
            UpdatePairSummaries();
        }
        public static void RemovePairSummary (PoloniexAPI.CurrencyPair pair) {
            PairSummariesManager.RemovePairSummary(pair);
            UpdatePairSummaries();
        }
        public static void ClearPairSummaries () {
            PairSummariesManager.ClearPairSummaries();
            UpdatePairSummaries();
        }

        public static bool MarkPairUser (PoloniexAPI.CurrencyPair pair) {
            if (PairSummariesManager.MarkPairSummary(pair, true)) {
                UpdatePairSummaries();
                return true;
            }
            else return false;
        }
        public static bool UnmarkPairUser (PoloniexAPI.CurrencyPair pair) {
            if (PairSummariesManager.MarkPairSummary(pair, false)) {
                UpdatePairSummaries();
                return true;
            }
            else return false;
        }
    }
}

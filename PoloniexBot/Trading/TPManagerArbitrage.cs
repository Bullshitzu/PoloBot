using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoloniexBot.Trading.Rules;
using PoloniexAPI.MarketTools;
using PoloniexAPI;

namespace PoloniexBot.Trading {
    class TPManagerArbitrage : IDisposable, IComparable<TPManagerArbitrage> {

        // -----------------------------------------

        private class TransactionData {

            public CurrencyPair pair;
            public OrderType type;
            public double baseAmount;
            public double quoteAmount;
            public double price;

            public TransactionData (CurrencyPair pair, OrderType type, double baseAmount, double quoteAmount, double price) {
                this.pair = pair;
                this.type = type;
                this.baseAmount = baseAmount;
                this.quoteAmount = quoteAmount;
                this.price = price;
            }
        }

        // -----------------------------------------

        public CurrencyPair pair1;
        public CurrencyPair pair2;
        public CurrencyPair pair3;

        public TPManagerArbitrage (string quote, string base1, string base2) {
            this.pair1 = new CurrencyPair(base1, quote);
            this.pair2 = new CurrencyPair(base2, quote);
            this.pair3 = new CurrencyPair(base1, base2);

            GUI.GUIManager.AddStrategyScreenPair(pair2);
        }

        // -----------------------------------------

        public double PairScore = 0;

        private static long LastTradeTime = 0;
        private static long MinimumTradeDelay = 30;

        private const double FeeMult = 0.9975;
        private const double MinimumTradeAmount = 0.0001;

        // -----------------------------------------

        public void Setup () {
            // todo: ?
        }

        public void Update () {

            if (CheckRemainders()) return;

            // pull new orders from 3 strats (price AND amount)
            IOrderBook ob1 = Data.Store.GetOrderBook(pair1);
            IOrderBook ob2 = Data.Store.GetOrderBook(pair2);
            IOrderBook ob3 = Data.Store.GetOrderBook(pair3);

            // todo: update GUI pair summaries

            // simulate a triangular arbitrage and execute if possible / profitable
            TransactionData[] tData;
            if (SimulateTriArbitrage(ob1, ob2, ob3, out tData)) {
                if (PairScore <= 0) return;

                long currTimestamp = Utility.DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
                if (currTimestamp - LastTradeTime < MinimumTradeDelay) return;

                // ---------------------------------------------

                LastTradeTime = currTimestamp;
                ManagerArbitrage.ShouldUpdateWallet = true;


                // step 1
                if (ExecuteTransaction(tData[0])) {
                    
                    Utility.TradeTracker.ReportBuy(tData[0].pair, tData[0].quoteAmount, tData[0].price, currTimestamp);
                    Utility.TradeTracker.SetOrderData(tData[0].pair, 0, tData[0].price);

                    Thread.Sleep(ManagerArbitrage.APICallPeriod);
                }
                else return;


                // step 2
                if (ExecuteTransaction(tData[1])) {
                    Thread.Sleep(ManagerArbitrage.APICallPeriod);
                }
                else return;


                // step 3
                if (ExecuteTransaction(tData[2])) {
                    Utility.TradeTracker.ReportSell(tData[0].pair, tData[0].quoteAmount, tData[0].price, currTimestamp);
                }
                else return;
            }
        }

        public bool CheckRemainders () {

            // todo: check if you have remaining coins from failed transactions and if you can sell them
            // eth > 0 or quote > 0 then sell them





            // todo: return true if made any transactions and set lastTradeTime
            return false;
        }

        private bool SimulateTriArbitrage (IOrderBook ob1, IOrderBook ob2, IOrderBook ob3, out TransactionData[] tData) {
            bool canExecute = true;

            List<TransactionData> transactionData = new List<TransactionData>();

            double tempBaseAmount = 0;
            double tempQuoteAmount = 0;
            double tempPrice = 0;

            double currAmount = FindInitialBaseAmount(ManagerArbitrage.GetWalletState(pair1.BaseCurrency), ob1, ob2);
            double startAmount = currAmount;

            // STEP 1

            tempBaseAmount = currAmount;
            // tempPrice = ob1.SellOrders.First().PricePerCoin;
            tempPrice = ob1.BuyOrders.First().PricePerCoin;
            currAmount /= tempPrice;
            tempQuoteAmount = currAmount;

            transactionData.Add(new TransactionData(pair1, OrderType.Buy, tempBaseAmount, tempQuoteAmount, tempPrice));

            currAmount *= FeeMult;

            // STEP 2

            tempQuoteAmount = currAmount;
            tempPrice = ob2.BuyOrders.First().PricePerCoin;
            currAmount *= tempPrice;
            tempBaseAmount = currAmount;

            transactionData.Add(new TransactionData(pair2, OrderType.Sell, tempBaseAmount, tempQuoteAmount, tempPrice));

            currAmount *= FeeMult;

            // STEP 3
            
            tempQuoteAmount = currAmount;
            // tempPrice = ob3.BuyOrders.First().PricePerCoin;
            tempPrice = ob3.SellOrders.First().PricePerCoin;
            currAmount *= tempPrice;
            tempBaseAmount = currAmount;

            transactionData.Add(new TransactionData(pair3, OrderType.Sell, tempBaseAmount, tempQuoteAmount, tempPrice));

            currAmount *= FeeMult;

            // Update GUI

            PairScore = ((currAmount / startAmount) - 1) * 100;

            Dictionary<string, double> tempDictionary = new Dictionary<string, double>();
            tempDictionary.Add("score", PairScore);

            GUI.GUIManager.UpdateStrategyScreenPair(pair2, tempDictionary);

            // Check validity (minimum trade amounts etc.)

            for (int i = 0; i < transactionData.Count; i++) {
                if (transactionData[i].baseAmount < MinimumTradeAmount) canExecute = false;
                if (transactionData[i].quoteAmount < MinimumTradeAmount) canExecute = false;
            }

            if (PairScore <= ManagerArbitrage.MinProfitMargin) canExecute = false;

            tData = transactionData.ToArray();

            if (PairScore > 0) Console.WriteLine("Score: " + PairScore.ToString("F4") + " - CanExecute: " + canExecute);

            return canExecute;
        }

        private double FindInitialBaseAmount (double baseAmountWallet, IOrderBook ob1, IOrderBook ob2) {

            double ob2Base = ob2.BuyOrders.First().AmountBase;
            ob2Base *= ob1.SellOrders.First().PricePerCoin;

            if (baseAmountWallet > ob2Base) baseAmountWallet = ob2Base;

            return baseAmountWallet;
        }

        private bool ExecuteTransaction (TransactionData td, bool modifyPrice = false) {
            try {
                ulong id = PoloniexBot.ClientManager.client.Trading.PostOrderAsync(td.pair, td.type, td.price, td.quoteAmount, modifyPrice).Result;

                if (id == 0) {
                    Console.WriteLine("Error executing transaction.");
                }
                else return true;
            }
            catch (Exception e) {
                Console.WriteLine("Error executing transaction: " + e.Message);
            }

            return false;
        }

        public void Dispose () {
            // todo: cleanup?
        }

        public int CompareTo (TPManagerArbitrage other) {
            return this.PairScore.CompareTo(other.PairScore);
        }
    }
}

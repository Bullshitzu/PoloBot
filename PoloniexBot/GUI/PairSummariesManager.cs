using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;

namespace PoloniexBot.GUI {
    static class PairSummariesManager {

        public class PairSummary : IComparable<PairSummary> {

            public CurrencyPair Pair;
            public TickerChangedEventArgs[] Tickers;
            public bool MarkedUser;
            public double Volume24;
            public bool Blocked = false;

            public PairSummary (CurrencyPair pair, TickerChangedEventArgs[] tickers, double volume) {
                this.Pair = pair;
                this.Tickers = tickers;
                this.Volume24 = volume;
                MarkedUser = false;
            }
            public PairSummary (PairSummary old, TickerChangedEventArgs[] tickers, double volume) {
                this.Pair = old.Pair;
                this.Tickers = tickers;
                this.Volume24 = volume;
                this.MarkedUser = old.MarkedUser;
                this.Blocked = old.Blocked;
            }
            public PairSummary (PairSummary old, bool markedUser) {
                this.Pair = old.Pair;
                this.Tickers = old.Tickers;
                this.Volume24 = old.Volume24;
                this.MarkedUser = markedUser;
                this.Blocked = old.Blocked;
            }

            public int CompareTo (PairSummary other) {
                if (MarkedUser != other.MarkedUser) return MarkedUser.CompareTo(other.MarkedUser);
                return Volume24.CompareTo(other.Volume24);
            }
        }

        static PairSummariesManager () {
            AllSummaries = new List<PairSummary>();
        }

        private static List<PairSummary> AllSummaries;

        public static void SetPairSummary (CurrencyPair pair, TickerChangedEventArgs[] tickers, double volume) {
            lock (AllSummaries) {
                for (int i = 0; i < AllSummaries.Count; i++) {
                    if (AllSummaries[i].Pair == pair) {
                        AllSummaries[i] = new PairSummary(AllSummaries[i], tickers, volume);
                        return;
                    }
                }
                AllSummaries.Add(new PairSummary(pair, tickers, volume));
            }
        }
        public static void RemovePairSummary (CurrencyPair pair) {
            lock (AllSummaries) {
                for (int i = 0; i < AllSummaries.Count; i++) {
                    if (AllSummaries[i].Pair == pair) {
                        AllSummaries.RemoveAt(i);
                        return;
                    }
                }
            }
        }
        public static void ClearPairSummaries () {
            lock (AllSummaries) {
                AllSummaries.Clear();
            }
        }
        public static bool MarkPairSummary (CurrencyPair pair, bool marked) {
            lock (AllSummaries) {
                for (int i = 0; i < AllSummaries.Count; i++) {
                    if (AllSummaries[i].Pair == pair) {
                        AllSummaries[i] = new PairSummary(AllSummaries[i], marked);
                        return true;
                    }
                }
                return false;
            }
        }
        public static bool SetPairBlock (CurrencyPair pair, bool state) {
            lock (AllSummaries) {
                for (int i = 0; i < AllSummaries.Count; i++) {
                    if (AllSummaries[i].Pair == pair) {
                        AllSummaries[i].Blocked = state;
                        return true;
                    }
                }
                return false;
            }
        }

        public static PairSummary[] GetPairsSorted () {
            return AllSummaries.ToArray();
        }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PoloniexAPI.TradingTools {
    public class TradingCustom : ITrading {
        private ApiWebClient ApiWebClient { get; set; }

        internal TradingCustom (ApiWebClient apiWebClient) {
            ApiWebClient = apiWebClient;
        }

        private IList<IOrder> GetOpenOrders (CurrencyPair currencyPair) {
            try {
                var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair }
            };

                var data = PostData<IList<Order>>("returnOpenOrders", postData);
                if (data == null || data.Count == 0) return null;

                IList<IOrder> list = new List<IOrder>();
                for (int i = 0; i < data.Count; i++) {
                    list.Add(data[i]);
                }
                return list;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return null;
            }
        }

        private IList<ITrade> GetTrades (CurrencyPair currencyPair, DateTime startTime, DateTime endTime) {
            try {
                var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

                var data = PostData<IList<Trade>>("returnTradeHistory", postData);
                return (IList<ITrade>)data;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return null;
            }
        }

        private ulong PostOrder (CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote) {

            switch (type) {
                case OrderType.Buy:
                    pricePerCoin *= 1.05f;
                    break;
                case OrderType.Sell:
                    pricePerCoin *= 0.95f;
                    break;
            }

            if (amountQuote <= PoloniexBot.Trading.Rules.RuleMinimumBaseAmount.MinimumAllowedTradeAmount) return 0;
            if (amountQuote * pricePerCoin <= PoloniexBot.Trading.Rules.RuleMinimumBaseAmount.MinimumAllowedTradeAmount) return 0;
            
            try {
                pricePerCoin = double.Parse(pricePerCoin.ToString("F8"));
                var postData = new Dictionary<string, object> {
                    { "currencyPair", currencyPair },
                    { "rate", pricePerCoin.ToStringNormalized() },
                    { "amount", amountQuote.ToStringNormalized() }
                };

                var data = PostData<JObject>(type.ToStringNormalized(), postData);
                return data.Value<ulong>("orderNumber");
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportError("Error making sale: " + currencyPair + " - " + e.Message, e);
                return 0;
            }
        }

        private ulong MoveOrder (ulong orderId, double pricePerCoin, double amountQuote) {
            try {
                var postData = new Dictionary<string, object> {
                { "orderNumber", orderId },
                { "rate", pricePerCoin.ToStringNormalized() },
                { "amount", amountQuote.ToStringNormalized() }
            };

                var data = PostData<JObject>("moveOrder", postData);
                return data.Value<ulong>("orderNumber");
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return 0;
            }
        }
        private ulong MoveOrder (ulong orderId, double pricePerCoin) {
            try {
                var postData = new Dictionary<string, object> {
                { "orderNumber", orderId },
                { "rate", pricePerCoin.ToStringNormalized() }
            };

                var data = PostData<JObject>("moveOrder", postData);
                return data.Value<ulong>("orderNumber");
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return 0;
            }
        }

        private bool DeleteOrder (CurrencyPair currencyPair, ulong orderId) {
            try {
                var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "orderNumber", orderId }
            };

                var data = PostData<JObject>("cancelOrder", postData);
                return data.Value<byte>("success") == 1;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return false;
            }
        }

        public Task<IList<IOrder>> GetOpenOrdersAsync (CurrencyPair currencyPair) {
            return Task.Factory.StartNew(() => GetOpenOrders(currencyPair));
        }

        public Task<IList<ITrade>> GetTradesAsync (CurrencyPair currencyPair, DateTime startTime, DateTime endTime) {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, startTime, endTime));
        }

        public Task<IList<ITrade>> GetTradesAsync (CurrencyPair currencyPair) {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<ulong> PostOrderAsync (CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote) {
            return Task.Factory.StartNew(() => PostOrder(currencyPair, type, pricePerCoin, amountQuote));
        }

        public Task<ulong> MoveOrderAsync (ulong orderId, double pricePerCoin, double amountQuote) {
            return Task.Factory.StartNew(() => MoveOrder(orderId, pricePerCoin, amountQuote));
        }

        public Task<ulong> MoveOrderAsync (ulong orderId, double pricePerCoin) {
            return Task.Factory.StartNew(() => MoveOrder(orderId, pricePerCoin));
        }

        public Task<bool> DeleteOrderAsync (CurrencyPair currencyPair, ulong orderId) {
            return Task.Factory.StartNew(() => DeleteOrder(currencyPair, orderId));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T> (string command, Dictionary<string, object> postData) {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}

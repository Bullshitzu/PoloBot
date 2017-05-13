using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PoloniexAPI.TradingTools {
    public class TradingSimulated : ITrading {
        private ApiWebClient ApiWebClient { get; set; }

        internal TradingSimulated (ApiWebClient apiWebClient) {
            ApiWebClient = apiWebClient;

            openOrders = new Dictionary<CurrencyPair, IList<IOrder>>();
        }

        // -----------------------------------

        private IDictionary<CurrencyPair, IList<IOrder>> openOrders;

        // -----------------------------------

        private IList<IOrder> GetOpenOrders (CurrencyPair currencyPair) {
            IList<IOrder> orders;
            if (openOrders.TryGetValue(currencyPair, out orders)) {
                return orders;
            }
            return null;
        }

        private IList<ITrade> GetTrades (CurrencyPair currencyPair, DateTime startTime, DateTime endTime) {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

            var data = PostData<IList<Trade>>("returnTradeHistory", postData);
            return (IList<ITrade>)data;
        }

        private ulong PostOrder (CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote) {
            PoloniexBot.Simulation.PostOrder(currencyPair, type, pricePerCoin, amountQuote);
            return 1;
        }

        private bool DeleteOrder (CurrencyPair currencyPair, ulong orderId) {
            return true;
        }

        // -----------------------------------

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

        public Task<bool> DeleteOrderAsync (CurrencyPair currencyPair, ulong orderId) {
            return Task.Factory.StartNew(() => DeleteOrder(currencyPair, orderId));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T> (string command, Dictionary<string, object> postData) {
            return ApiWebClient.PostData<T>(command, postData);
        }


        public Task<ulong> MoveOrderAsync (ulong orderNumber, double pricePerCoin, double amountQuote) {
            throw new NotImplementedException();
        }

        public Task<ulong> MoveOrderAsync (ulong orderNumber, double pricePerCoin) {
            throw new NotImplementedException();
        }
    }
}

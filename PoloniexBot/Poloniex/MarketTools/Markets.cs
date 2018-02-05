using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PoloniexAPI.MarketTools {
    public class Markets : IMarkets {
        private ApiWebClient ApiWebClient { get; set; }

        internal Markets (ApiWebClient apiWebClient) {
            ApiWebClient = apiWebClient;
        }

        private IDictionary<CurrencyPair, IMarketData> GetSummary () {
            try {
                var data = GetData<IDictionary<string, MarketData>>("returnTicker");
                return data.ToDictionary(
                    x => CurrencyPair.Parse(x.Key),
                    x => (IMarketData)x.Value
                );
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return null;
            }
        }

        private IDictionary<CurrencyPair, IOrderBook> GetOpenOrders (uint depth) {
            try {
                var data = GetData<IDictionary<string, OrderBook>>(
                    "returnOrderBook",
                    "currencyPair=all",
                    "depth=" + depth
                );
                return data.ToDictionary(
                    x => CurrencyPair.Parse(x.Key),
                    x => (IOrderBook)x.Value
                );
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return null;
            }
        }

        private IOrderBook GetOpenOrders (CurrencyPair currencyPair, uint depth) {
            try {
                var data = GetData<OrderBook>(
                    "returnOrderBook",
                    "currencyPair=" + currencyPair,
                    "depth=" + depth
                );
                return data;
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return null;
            }
        }

        private IList<ITrade> GetTrades (CurrencyPair currencyPair) {
            try {
                var data = GetData<IList<Trade>>(
                    "returnTradeHistory",
                    "currencyPair=" + currencyPair
                );
                return new List<ITrade>(data);
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return null;
            }
        }

        private IList<ITrade> GetTrades (CurrencyPair currencyPair, DateTime startTime, DateTime endTime) {
            try {
                var data = GetData<IList<Trade>>(
                    "returnTradeHistory",
                    "currencyPair=" + currencyPair,
                    "start=" + Helper.DateTimeToUnixTimeStamp(startTime),
                    "end=" + Helper.DateTimeToUnixTimeStamp(endTime)
                    );
                return new List<ITrade>(data);
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return null;
            }
        }

        private IList<IMarketChartData> GetChartData (CurrencyPair currencyPair, MarketPeriod period, DateTime startTime, DateTime endTime) {
            try {
                var data = GetData<IList<MarketChartData>>(
                    "returnChartData",
                    "currencyPair=" + currencyPair,
                    "start=" + Helper.DateTimeToUnixTimeStamp(startTime),
                    "end=" + Helper.DateTimeToUnixTimeStamp(endTime),
                    "period=" + (int)period
                );
                return new List<IMarketChartData>(data);
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return null;
            }
        }

        public Task<IDictionary<CurrencyPair, IMarketData>> GetSummaryAsync () {
            return Task.Factory.StartNew(() => GetSummary());
        }

        public Task<IOrderBook> GetOpenOrdersAsync (CurrencyPair currencyPair, uint depth) {
            return Task.Factory.StartNew(() => GetOpenOrders(currencyPair, depth));
        }

        public Task<IDictionary<CurrencyPair, IOrderBook>> GetOpenOrdersAsync (uint depth) {
            return Task.Factory.StartNew(() => GetOpenOrders(depth));
        }

        public Task<IList<ITrade>> GetTradesAsync (CurrencyPair currencyPair) {
            return Task.Factory.StartNew(() => GetTrades(currencyPair));
        }

        public Task<IList<ITrade>> GetTradesAsync (CurrencyPair currencyPair, DateTime startTime, DateTime endTime) {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, startTime, endTime));
        }

        public Task<IList<IMarketChartData>> GetChartDataAsync (CurrencyPair currencyPair, MarketPeriod period, DateTime startTime, DateTime endTime) {
            return Task.Factory.StartNew(() => GetChartData(currencyPair, period, startTime, endTime));
        }

        public Task<IList<IMarketChartData>> GetChartDataAsync (CurrencyPair currencyPair, MarketPeriod period) {
            return Task.Factory.StartNew(() => GetChartData(currencyPair, period, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<IList<IMarketChartData>> GetChartDataAsync (CurrencyPair currencyPair) {
            return Task.Factory.StartNew(() => GetChartData(currencyPair, MarketPeriod.Minutes30, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetData<T> (string command, params object[] parameters) {
            return ApiWebClient.GetData<T>(Helper.ApiUrlHttpsRelativePublic + command, parameters);
        }
    }
}

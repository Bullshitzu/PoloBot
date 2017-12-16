using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Utility;

namespace Utility {
    static class WebApiCustom {

        static string QueryGet (string address) {

            APICallTracker.ReportApiCall();

            Log.Manager.LogNetSent(address);

            try {
                UriBuilder builder = new UriBuilder(address);
                HttpWebRequest request = WebRequest.CreateHttp(builder.Uri);

                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;

                string s;
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream())) {
                    s = reader.ReadToEnd();
                }

                Log.Manager.LogNetReceived(s);

                return s;
            }
            catch (Exception e) {
                PoloniexBot.CLI.Manager.PrintError(e.Message);
                return "";
            }
        }

        // --------------------

        public static List<PoloniexAPI.MarketTools.ITrade> GetTrades (PoloniexAPI.CurrencyPair pair) {
            string response = QueryGet("https://poloniex.com/public?command=returnTradeHistory&currencyPair=" + pair.ToString());
            return ParseTrades(response);
        }
        public static List<PoloniexAPI.MarketTools.ITrade> GetTrades (PoloniexAPI.CurrencyPair pair, int startTime, int endTime) {
            startTime = (int)DateTimeHelper.GetServerTime(startTime);
            endTime = (int)DateTimeHelper.GetServerTime(endTime);
            string response = QueryGet("https://poloniex.com/public?command=returnTradeHistory&currencyPair=" + pair + "&start=" + startTime + "&end=" + endTime);
            return ParseTrades(response);
        }
        public static List<PoloniexAPI.MarketTools.ITrade> GetTrades (PoloniexAPI.CurrencyPair pair, DateTime startTime, DateTime endTime) {
            int startTimeUNIX = (int)DateTimeHelper.DateTimeToUnixTimestamp(startTime);
            int endTimeUNIX = (int)DateTimeHelper.DateTimeToUnixTimestamp(endTime);
            return GetTrades(pair, startTimeUNIX, endTimeUNIX);
        }

        public static IList<PoloniexAPI.MarketTools.IMarketChartData> GetChartData (
            PoloniexAPI.CurrencyPair currencyPair,
            PoloniexAPI.MarketTools.MarketPeriod period,
            DateTime startTime, DateTime endTime) {
            int startTimestamp = (int)Utility.DateTimeHelper.DateTimeToUnixTimestamp(startTime);
            int endTimestamp = (int)Utility.DateTimeHelper.DateTimeToUnixTimestamp(endTime);

            startTimestamp = (int)Utility.DateTimeHelper.GetServerTime(startTimestamp);
            endTimestamp = (int)Utility.DateTimeHelper.GetServerTime(endTimestamp);

            string response = QueryGet("https://poloniex.com/public?command=returnChartData&currencyPair=" + currencyPair +
                    "&start=" + startTimestamp +
                    "&end=" + endTimestamp +
                    "&period=" + (int)period);

            return ParseChartData(response);
        }

        // --------------------

        #region Trades
        static List<PoloniexAPI.MarketTools.ITrade> ParseTrades (string json) {
            try {
                List<Trade> trades = JsonConvert.DeserializeObject<List<Trade>>(json);
                List<PoloniexAPI.MarketTools.ITrade> tradeList = new List<PoloniexAPI.MarketTools.ITrade>();
                for (int i = 0; i < trades.Count; i++) {
                    tradeList.Add(new PoloniexAPI.MarketTools.Trade(trades[i].date, trades[i].type, trades[i].rate, trades[i].amount, trades[i].total));
                }
                return tradeList;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent("Error parsing JSON to trades - " + e.Message + " - " + json);
                return null;
            }
        }
        public class Trade {
            public string date { get; set; }
            public string type { get; set; }
            public string rate { get; set; }
            public string amount { get; set; }
            public string total { get; set; }
        }
        #endregion

        #region Chart Data
        static IList<PoloniexAPI.MarketTools.IMarketChartData> ParseChartData (string json) {
            IList<ChartPoint> points = JsonConvert.DeserializeObject<List<ChartPoint>>(json);
            List<PoloniexAPI.MarketTools.IMarketChartData> chartData = new List<PoloniexAPI.MarketTools.IMarketChartData>();
            if (points == null) return null;
            if (points.Count == 0) return null;
            for (int i = 0; i < points.Count; i++) {
                chartData.Add(new PoloniexAPI.MarketTools.MarketChartData(
                    (ulong)Utility.DateTimeHelper.GetClientTime(points[i].date),
                    points[i].open,
                    points[i].close,
                    points[i].high,
                    points[i].low,
                    points[i].volume,
                    points[i].quoteVolume,
                    points[i].weightedAverage));
            }
            return chartData;
        }
        public class ChartPoint {
            public int date { get; set; }
            public double high { get; set; }
            public double low { get; set; }
            public double open { get; set; }
            public double close { get; set; }
            public double volume { get; set; }
            public double quoteVolume { get; set; }
            public double weightedAverage { get; set; }
        }
        #endregion
    }
}

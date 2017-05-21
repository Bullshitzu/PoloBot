using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace PoloniexBot {
    public static class JsonParser {

        public static IDictionary<string, PoloniexAPI.WalletTools.IBalance> ParseBalances (string text) {
            try {
                System.Text.Json.JsonParser parser = new System.Text.Json.JsonParser();
                KeyValuePair<string, Balance>[] data = parser.Parse<Dictionary<string, Balance>>(text).ToArray();
                IDictionary<string, PoloniexAPI.WalletTools.IBalance> returnData = new Dictionary<string, PoloniexAPI.WalletTools.IBalance>();
                for (int i = 0; i < data.Length; i++) {
                    returnData.Add(new KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>(data[i].Key, (PoloniexAPI.WalletTools.Balance)data[i].Value));
                }
                return returnData;
            }
            catch (Exception e) {
                Console.WriteLine("JSON Parser, line 22");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static Trading.OrderLive ParseOrderLive (string text) {
            System.Text.Json.JsonParser parser = new System.Text.Json.JsonParser();
            OrderLive order = parser.Parse<OrderLive>(text);
            return order;
        }

        private class Balance {
            public string available { get; set; }
            public string onOrders { get; set; }
            public string btcValue { get; set; }

            public static implicit operator PoloniexAPI.WalletTools.Balance (Balance b) {
                System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
                nfi.NumberGroupSeparator = " ";
                nfi.NumberDecimalSeparator = ".";
                return new PoloniexAPI.WalletTools.Balance(double.Parse(b.available, nfi), double.Parse(b.onOrders, nfi), double.Parse(b.btcValue, nfi));
            }
        }

        private class OrderLive {
            public string type { get; set; }
            public OrderLiveData data { get; set; }

            public static implicit operator Trading.OrderLive (OrderLive order) {
                Trading.OrderLive ol = new Trading.OrderLive(order.type, order.data.type, order.data.amount, order.data.rate);
                return ol;
            }
        }
        private class OrderLiveData {
            public string type { get; set; }
            public string rate { get; set; }
            public string amount { get; set; }
        }

    }
}

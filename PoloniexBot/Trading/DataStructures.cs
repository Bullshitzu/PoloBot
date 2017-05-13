using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Trading {

    public enum MarketAction {
        Hold,
        Sell,
        Buy,
    }

    public enum OrderLiveType {
        Add,
        Modify,
        Remove
    }

    public class OrderLive : IComparable<OrderLive> {

        public OrderLiveType bookType;
        public MarketAction orderType;
        public double amount;
        public double rate;

        public OrderLive (string bookType, string orderType, string amount, string rate) {
            if (bookType == "orderBookModify") this.bookType = OrderLiveType.Modify;
            else if (bookType == "orderBookRemove") this.bookType = OrderLiveType.Remove;
            else if (bookType == "newTrade") this.bookType = OrderLiveType.Add;
            else throw new Exception("Unknown BookOrder type: " + bookType);

            if (orderType == "ask" || orderType == "sell") this.orderType = MarketAction.Sell;
            else if (orderType == "bid" || orderType == "buy") this.orderType = MarketAction.Buy;
            else throw new Exception("Unknown order type: " + orderType);

            if (string.IsNullOrEmpty(amount)) this.amount = 0;
            else this.amount = double.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

            this.rate = double.Parse(rate, System.Globalization.CultureInfo.InvariantCulture);
        }

        public OrderLive (OrderLiveType bookType, MarketAction orderType, double amount, double rate) {
            this.bookType = bookType;
            this.orderType = orderType;
            this.amount = amount;
            this.rate = rate;
        }

        public int CompareTo (OrderLive other) {
            return this.rate.CompareTo(other.rate);
        }
    }
}

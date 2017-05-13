using System;

namespace PoloniexAPI.TradingTools {
    public interface ITrade : IOrder {
        DateTime Time { get; }
    }
}

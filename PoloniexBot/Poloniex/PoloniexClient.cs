using PoloniexAPI.LiveTools;
using PoloniexAPI.MarketTools;
using PoloniexAPI.TradingTools;
using PoloniexAPI.WalletTools;

namespace PoloniexAPI {
    public sealed class PoloniexClient {
        /// <summary>Represents the authenticator object of the client.</summary>
        public IAuthenticator Authenticator { get; private set; }

        /// <summary>A class which contains market tools for the client.</summary>
        public IMarkets Markets { get; private set; }
        /// <summary>A class which contains trading tools for the client.</summary>
        public ITrading Trading { get; private set; }
        /// <summary>A class which contains wallet tools for the client.</summary>
        public IWallet Wallet { get; private set; }
        /// <summary>A class which represents live data fetched automatically from the server.</summary>
        public ILive Live { get; private set; }

        /// <summary>Creates a new instance of Poloniex API .NET's client service.</summary>
        /// <param name="publicApiKey">Your public API key.</param>
        /// <param name="privateApiKey">Your private API key.</param>
        public PoloniexClient (string publicApiKey, string privateApiKey) {
            var apiWebClient = new ApiWebClient(Helper.ApiUrlHttpsBase);

            Authenticator = new Authenticator(apiWebClient, publicApiKey, privateApiKey);

            Markets = new MarketsCustom (apiWebClient);
            Trading = new TradingCustom (apiWebClient);
            Wallet = new WalletCustom (apiWebClient);
            Live = new LiveCustom();
        }

        /// <summary>Creates a new, unauthorized instance of Poloniex API .NET's client service.</summary>
        public PoloniexClient () : this("", "") { }
    }
}

using PoloniexAPI.MarketTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using WampSharp.V2;
using WampSharp.V2.Realm;

namespace PoloniexAPI.LiveTools {
    public class LiveCustom : ILive {
        private const string SubjectNameTicker = "ticker";
        private const string SubjectNameTrollbox = "trollbox";

        public event EventHandler<TickerChangedEventArgs> OnTickerChanged;
        public event EventHandler<TrollboxMessageEventArgs> OnTrollboxMessage;

        private IWampChannel WampChannel { get; set; }
        private Task WampChannelOpenTask { get; set; }

        private readonly IDictionary<string, IDisposable> _activeSubscriptions = new Dictionary<string, IDisposable>();
        private IDictionary<string, IDisposable> ActiveSubscriptions {
            get { return _activeSubscriptions; }
        }

        private readonly ObservableDictionary<CurrencyPair, MarketData> _tickers = new ObservableDictionary<CurrencyPair, MarketData>();
        public ObservableDictionary<CurrencyPair, MarketData> Tickers {
            get { return _tickers; }
        }

        public void Start () {
            try {
                WampChannel = new DefaultWampChannelFactory().CreateJsonChannel(Helper.ApiUrlWssBase, "realm1");
                WampChannel.RealmProxy.Monitor.ConnectionBroken += OnConnectionBroken;
                WampChannel.RealmProxy.Monitor.ConnectionError += OnConnectionError;
                WampChannelOpenTask = WampChannel.Open();
            }
            catch (Exception e) {
                Console.WriteLine("WAMP EXCEPTION (Start): " + e.Message);
            }
        }

        void OnConnectionError (object sender, WampSharp.Core.Listener.WampConnectionErrorEventArgs e) {
            PoloniexBot.Trading.Manager.RefreshTickersManually = true;
        }

        public void Stop () {
            Console.WriteLine("STOP CALLED");

            try {
                foreach (var subscription in ActiveSubscriptions.Values) {
                    subscription.Dispose();
                }
                ActiveSubscriptions.Clear();

                PoloniexBot.Trading.Manager.Stop();
                PoloniexBot.Trading.Manager.ClearAllPairs();

                WampChannel.Close();

                Utility.NetworkStatus.BootDown();
            }
            catch (Exception ex) {
                Console.WriteLine("WAMP EXCEPTION (Stop): " + ex.Message);
            }
        }

        
        private void OnConnectionBroken (object sender, WampSessionCloseEventArgs e) {

            Console.WriteLine("OnConnectionBroken CALLED");

            try {
                foreach (var subscription in ActiveSubscriptions.Values) {
                    subscription.Dispose();
                }
                ActiveSubscriptions.Clear();

                PoloniexBot.Trading.Manager.Stop();
                PoloniexBot.Trading.Manager.ClearAllPairs();

                Utility.NetworkStatus.BootDown();
            }
            catch (Exception ex) {
                Console.WriteLine("WAMP EXCEPTION (OnConnectionBroken): " + ex.Message);
            }
        }

        public async Task SubscribeToTickerAsync () {
            try {
                if (!ActiveSubscriptions.ContainsKey(SubjectNameTicker)) {
                    await WampChannelOpenTask;
                    ActiveSubscriptions.Add(SubjectNameTicker, WampChannel.RealmProxy.Services.GetSubject(SubjectNameTicker).Subscribe(x => ProcessMessageTicker(x.Arguments)));
                }
            }
            catch (Exception ex) {
                Console.WriteLine("WAMP EXCEPTION (SubscribeToTickerAsync): " + ex.Message);
            }
        }

        public async Task SubscribeToTrollboxAsync () {
            try {
                if (!ActiveSubscriptions.ContainsKey(SubjectNameTrollbox)) {
                    await WampChannelOpenTask;
                    ActiveSubscriptions.Add(SubjectNameTrollbox, WampChannel.RealmProxy.Services.GetSubject(SubjectNameTrollbox).Subscribe(x => ProcessMessageTrollbox(x.Arguments)));
                }
            }
            catch (Exception ex) {
                Console.WriteLine("WAMP EXCEPTION (SubscribeToTrollboxAsync): " + ex.Message);
            }
        }

        private void ProcessMessageTicker (ISerializedValue[] arguments) {

            Utility.ModuleMonitor.ReportAlive("TickerStream", 180, () => { OnConnectionBroken(null, null); });

            try {
                var currencyPair = CurrencyPair.Parse(arguments[0].Deserialize<string>());
                var priceLast = arguments[1].Deserialize<double>();
                var orderTopSell = arguments[2].Deserialize<double>();
                var orderTopBuy = arguments[3].Deserialize<double>();
                var priceChangePercentage = arguments[4].Deserialize<double>();
                var volume24HourBase = arguments[5].Deserialize<double>();
                var volume24HourQuote = arguments[6].Deserialize<double>();
                var isFrozenInternal = arguments[7].Deserialize<byte>();

                var marketData = new MarketData {
                    PriceLast = priceLast,
                    OrderTopSell = orderTopSell,
                    OrderTopBuy = orderTopBuy,
                    PriceChangePercentage = priceChangePercentage,
                    Volume24HourBase = volume24HourBase,
                    Volume24HourQuote = volume24HourQuote,
                    IsFrozenInternal = isFrozenInternal
                };

                if (Tickers.ContainsKey(currencyPair)) {
                    Tickers[currencyPair] = marketData;
                }
                else {
                    Tickers.Add(currencyPair, marketData);
                }

                if (OnTickerChanged != null) OnTickerChanged(this, new TickerChangedEventArgs(currencyPair, marketData));
            }
            catch (Exception ex) {
                Console.WriteLine("WAMP EXCEPTION (ProcessMessageTicker): " + ex.Message);
            }
        }

        private void ProcessMessageTrollbox (ISerializedValue[] arguments) {
            try {
                if (OnTrollboxMessage == null) return;

                var messageNumber = arguments[1].Deserialize<ulong>();
                var senderName = arguments[2].Deserialize<string>();
                var messageText = HttpUtility.HtmlDecode(arguments[3].Deserialize<string>());
                var senderReputation = arguments.Length >= 5 ? arguments[4].Deserialize<uint?>() : null;

                OnTrollboxMessage(this, new TrollboxMessageEventArgs(senderName, senderReputation, messageNumber, messageText));
            }
            catch (Exception ex) {
                Console.WriteLine("WAMP EXCEPTION (ProcessMessageTrollbox): " + ex.Message);
            }
        }
    }
}

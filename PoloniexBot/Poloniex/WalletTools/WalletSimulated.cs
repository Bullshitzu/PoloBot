using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PoloniexAPI.WalletTools {
    public class WalletSimulated : IWallet {
        private ApiWebClient ApiWebClient { get; set; }

        // -----------------------------------

        private IDictionary<string, IBalance> balances;

        // -----------------------------------

        public void DoTransaction (CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote) {

            // base curr = btc

            Console.WriteLine("Transaction: " + currencyPair + " - " + type + " at rate " + pricePerCoin.ToString("F8") + ", Amount: " + amountQuote.ToString("F8"));

            double amountBase = pricePerCoin * amountQuote;

            if (type == OrderType.Buy) {
                IBalance baseCurrBalance = null;
                if (balances.TryGetValue(currencyPair.BaseCurrency, out baseCurrBalance)) {
                    balances.Remove(currencyPair.BaseCurrency);
                    double available = baseCurrBalance.QuoteAvailable - amountBase;
                    Balance b = new Balance(available, 0, available);
                    balances.Add(currencyPair.BaseCurrency, b);
                }
                else Console.WriteLine("Cannot do order because I'm missing sell currency");

                amountQuote *= 0.9975;

                IBalance quoteCurrBalance = null;
                if (balances.TryGetValue(currencyPair.QuoteCurrency, out quoteCurrBalance)) {
                    balances.Remove(currencyPair.QuoteCurrency);
                    double newAvailable = quoteCurrBalance.QuoteAvailable + amountQuote;
                    Balance b = new Balance(newAvailable, 0, newAvailable * pricePerCoin);
                    balances.Add(currencyPair.QuoteCurrency, b);
                }
                else {
                    Balance b = new Balance(amountQuote, 0, amountQuote * pricePerCoin);
                    balances.Add(currencyPair.QuoteCurrency, b);
                }
            }
            else {
                IBalance quoteCurrBalance = null;
                if (balances.TryGetValue(currencyPair.QuoteCurrency, out quoteCurrBalance)) {
                    balances.Remove(currencyPair.QuoteCurrency);
                    double newAvailable = quoteCurrBalance.QuoteAvailable - amountQuote;
                    Balance b = new Balance(newAvailable, 0, newAvailable * pricePerCoin);
                    balances.Add(currencyPair.QuoteCurrency, b);
                }
                else Console.WriteLine("Cannot do order because I'm missing sell currency");

                amountBase *= 0.9975;

                IBalance baseCurrBalance = null;
                if (balances.TryGetValue(currencyPair.BaseCurrency, out baseCurrBalance)) {
                    balances.Remove(currencyPair.BaseCurrency);
                    double available = baseCurrBalance.QuoteAvailable + amountBase;
                    Balance b = new Balance(available, 0, available);
                    balances.Add(currencyPair.BaseCurrency, b);
                }
                else {
                    Balance b = new Balance(amountBase, 0, amountBase);
                    balances.Add(currencyPair.BaseCurrency, b);
                }
            }

            PoloniexBot.ClientManager.RefreshWallet();
        }

        // -----------------------------------

        public void Reset () {
            balances = new Dictionary<string, IBalance>();
            Balance b = new Balance(1, 0, 1);
            balances.Add("BTC", b);
        }

        internal WalletSimulated (ApiWebClient apiWebClient) {
            ApiWebClient = apiWebClient;
            Reset();
        }

        private IDictionary<string, IBalance> GetBalances () {
            return balances;
        }

        private IDictionary<string, string> GetDepositAddresses () {
            throw new NotImplementedException();
        }

        private IDepositWithdrawalList GetDepositsAndWithdrawals (DateTime startTime, DateTime endTime) {
            throw new NotImplementedException();
        }

        private IGeneratedDepositAddress PostGenerateNewDepositAddress (string currency) {
            throw new NotImplementedException();
        }

        private void PostWithdrawal (string currency, double amount, string address, string paymentId) {
            throw new NotImplementedException();
        }

        // -----------------------------------

        public Task<IDictionary<string, IBalance>> GetBalancesAsync () {
            return Task.Factory.StartNew(() => GetBalances());
        }

        public Task<IDictionary<string, string>> GetDepositAddressesAsync () {
            return Task.Factory.StartNew(() => GetDepositAddresses());
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync (DateTime startTime, DateTime endTime) {
            return Task.Factory.StartNew(() => GetDepositsAndWithdrawals(startTime, endTime));
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync () {
            return Task.Factory.StartNew(() => GetDepositsAndWithdrawals(Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<IGeneratedDepositAddress> PostGenerateNewDepositAddressAsync (string currency) {
            return Task.Factory.StartNew(() => PostGenerateNewDepositAddress(currency));
        }

        public Task PostWithdrawalAsync (string currency, double amount, string address, string paymentId) {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, paymentId));
        }

        public Task PostWithdrawalAsync (string currency, double amount, string address) {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, null));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T> (string command, Dictionary<string, object> postData) {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}

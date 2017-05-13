using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PoloniexAPI.WalletTools {
    public class WalletCustom : IWallet {
        private ApiWebClient ApiWebClient { get; set; }

        internal WalletCustom (ApiWebClient apiWebClient) {
            ApiWebClient = apiWebClient;
        }

        private IDictionary<string, IBalance> GetBalances () {
            try {
                var postData = new Dictionary<string, object>();
                var response = ApiWebClient.PostData("returnCompleteBalances", postData);
                return PoloniexBot.JsonParser.ParseBalances(response);
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return null;
            }
        }

        private IDictionary<string, string> GetDepositAddresses () {
            try {
                var postData = new Dictionary<string, object>();
                var data = PostData<IDictionary<string, string>>("returnDepositAddresses", postData);
                return data;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return null;
            }
        }

        private IDepositWithdrawalList GetDepositsAndWithdrawals (DateTime startTime, DateTime endTime) {
            try {
                var postData = new Dictionary<string, object> {
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

                var data = PostData<DepositWithdrawalList>("returnDepositsWithdrawals", postData);
                return data;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return null;
            }
        }

        private IGeneratedDepositAddress PostGenerateNewDepositAddress (string currency) {
            try {
                var postData = new Dictionary<string, object> {
                { "currency", currency }
            };

                var data = PostData<IGeneratedDepositAddress>("generateNewAddress", postData);
                return data;
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
                return null;
            }
        }

        private void PostWithdrawal (string currency, double amount, string address, string paymentId) {
            try {
                var postData = new Dictionary<string, object> {
                { "currency", currency },
                { "amount", amount.ToStringNormalized() },
                { "address", address }
            };

                if (paymentId != null) {
                    postData.Add("paymentId", paymentId);
                }

                PostData<IGeneratedDepositAddress>("withdraw", postData);
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportErrorSilent(e);
            }
        }

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

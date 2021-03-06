using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PoloniexAPI {
    sealed class ApiWebClient {
        public string BaseUrl { get; private set; }

        private Authenticator _authenticator;
        public Authenticator Authenticator {
            private get { return _authenticator; }

            set {
                _authenticator = value;
                Encryptor.Key = Encoding.GetBytes(value.PrivateKey);
            }
        }

        private HMACSHA512 _encryptor = new HMACSHA512();
        public HMACSHA512 Encryptor {
            private get { return _encryptor; }
            set { _encryptor = value; }
        }

        public static readonly Encoding Encoding = Encoding.ASCII;
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        public ApiWebClient (string baseUrl) {
            BaseUrl = baseUrl;
        }

        public T GetData<T> (string command, params object[] parameters) {
            var relativeUrl = CreateRelativeUrl(command, parameters);
            var jsonString = QueryString(relativeUrl);
            var output = JsonSerializer.DeserializeObject<T>(jsonString);
            return output;
        }

        public T PostData<T> (string command, Dictionary<string, object> postData) {
            postData.Add("command", command);
            postData.Add("nonce", Helper.GetCurrentHttpPostNonce());
            var jsonString = PostString(Helper.ApiUrlHttpsRelativeTrading, postData.ToHttpPostString());

            var output = JsonSerializer.DeserializeObject<T>(jsonString);
            return output;
        }

        public string PostData (string command, Dictionary<string, object> postData) {
            postData.Add("command", command);
            postData.Add("nonce", Helper.GetCurrentHttpPostNonce());
            return PostString(Helper.ApiUrlHttpsRelativeTrading, postData.ToHttpPostString());
        }

        public string QueryString (string relativeUrl) {
            Utility.APICallTracker.ReportApiCall();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try {
                var request = CreateHttpWebRequest("GET", relativeUrl);

                string response = request.GetResponseString();

                Utility.Log.Manager.LogNetReceived(response);
                return response;
            }
            catch (System.Exception e) {
                Utility.ErrorLog.ReportError(e);
                return "";
            }
        }

        public string PostString (string relativeUrl, string postData) {
            Utility.APICallTracker.ReportApiCall();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try {
                var request = CreateHttpWebRequest("POST", relativeUrl);
                request.ContentType = "application/x-www-form-urlencoded";

                var postBytes = Encoding.GetBytes(postData);
                request.ContentLength = postBytes.Length;

                request.Headers["Key"] = Authenticator.PublicKey;
                request.Headers["Sign"] = Encryptor.ComputeHash(postBytes).ToStringHex();

                using (var requestStream = request.GetRequestStream()) {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                string response = request.GetResponseString();

                Utility.Log.Manager.LogNetReceived(response);

                return response;
            }
            catch (System.Exception e) {
                Utility.ErrorLog.ReportError(e);
                return "";
            }
        }

        public static string CreateRelativeUrl (string command, object[] parameters) {
            var relativeUrl = command;
            if (parameters.Length != 0) {
                relativeUrl += "&" + string.Join("&", parameters);
            }

            return relativeUrl;
        }

        private HttpWebRequest CreateHttpWebRequest (string method, string relativeUrl) {

            var request = WebRequest.CreateHttp(BaseUrl + relativeUrl);
            request.Method = method;
            request.UserAgent = "Poloniex API .NET v" + Helper.AssemblyVersionString;

            request.Timeout = Timeout.Infinite;

            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            Utility.Log.Manager.LogNetSent(BaseUrl + relativeUrl);

            return request;
        }
    }
}

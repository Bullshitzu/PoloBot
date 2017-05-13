using Newtonsoft.Json;
using System;

namespace PoloniexAPI.MarketTools {
    public class MarketChartData : IMarketChartData {
        [JsonProperty("date")]
        private ulong TimeInternal {
            set { Time = Helper.UnixTimeStampToDateTime(value); }
        }
        public DateTime Time { get; private set; }

        [JsonProperty("open")]
        public double Open { get; private set; }
        [JsonProperty("close")]
        public double Close { get; private set; }

        [JsonProperty("high")]
        public double High { get; private set; }
        [JsonProperty("low")]
        public double Low { get; private set; }

        [JsonProperty("volume")]
        public double VolumeBase { get; private set; }
        [JsonProperty("quoteVolume")]
        public double VolumeQuote { get; private set; }

        [JsonProperty("weightedAverage")]
        public double WeightedAverage { get; private set; }

        public MarketChartData (ulong time, double open, double close, double high, double low, double volumeBase, double volumeQuote, double avg) {
            this.TimeInternal = time;
            this.Open = open;
            this.Close = close;
            this.High = high;
            this.Low = low;
            this.VolumeBase = volumeBase;
            this.VolumeQuote = volumeQuote;
            this.WeightedAverage = avg;
        }

    }
}

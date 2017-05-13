using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.Windows.Controls {
    public partial class TickerFeed : MultiThreadControl {
        public TickerFeed () {
            InitializeComponent();
            TradedPairs = new Dictionary<PoloniexAPI.CurrencyPair, bool>();
        }

        struct CurrencyDataPair : IComparable<CurrencyDataPair> {
            public PoloniexAPI.CurrencyPair pair;
            public PoloniexAPI.MarketTools.IMarketData data;
            public CurrencyDataPair (PoloniexAPI.CurrencyPair pair, PoloniexAPI.MarketTools.IMarketData data) {
                this.pair = pair;
                this.data = data;
            }

            public int CompareTo (CurrencyDataPair other) {
                return this.data.Volume24HourBase.CompareTo(other.data.Volume24HourBase);
            }
        }
        CurrencyDataPair[] marketData;
        float[] lastUpdateChange;
        int[] lastUpdateDir;

        float scrollYOffset = 0;
        public float scrollYTarget = 0;
        float maxScrollYOffset = 0;

        Dictionary<PoloniexAPI.CurrencyPair, bool> TradedPairs;

        public void MarkPair (PoloniexAPI.CurrencyPair pair, bool traded) {
            if (TradedPairs.ContainsKey(pair)) TradedPairs.Remove(pair);
            if (traded) TradedPairs.Add(pair, traded);
        }

        public void UpdateMarketData () {
            KeyValuePair<PoloniexAPI.CurrencyPair, PoloniexAPI.MarketTools.IMarketData>[] pairs = Data.Store.MarketData.ToArray();
            List<CurrencyDataPair> pairsList = new List<CurrencyDataPair>();
            for (int i = 0; i < pairs.Length; i++) {
                pairsList.Add(new CurrencyDataPair(pairs[i].Key, pairs[i].Value));
            }
            pairsList.Sort();
            pairsList.Reverse();
            marketData = pairsList.ToArray();

            lastUpdateChange = new float[marketData.Length];
            lastUpdateDir = new int[marketData.Length];
        }

        public void Sort () {
            List<CurrencyDataPair> list = new List<CurrencyDataPair>(marketData);
            list.Sort();
            list.Reverse();
            marketData = list.ToArray();
        }

        public void UpdateTicker (PoloniexAPI.TickerChangedEventArgs e) {
            if (marketData == null) return;
            for (int i = 0; i < marketData.Length; i++) {
                double diff = e.MarketData.PriceLast - marketData[i].data.PriceLast;
                if (marketData[i].pair == e.CurrencyPair) {
                    marketData[i].data = e.MarketData;
                    lastUpdateChange[i] = 1;
                    lastUpdateDir[i] = diff.CompareTo(0);
                    break;
                }
            }
        }

        public void UpdateScroll () {
            float diff = scrollYTarget - scrollYOffset;
            diff /= 20;
            scrollYOffset += diff;
        }

        Brush brush1 = new SolidBrush(Color.FromArgb(142, 142, 142));
        Brush brush1Emphasis = new SolidBrush(Color.FromArgb(64, 164, 164));
        Brush brush2 = new SolidBrush(Color.FromArgb(142, 142, 142));
        Brush backBrush1 = new SolidBrush(Color.FromArgb(30, 30, 30));
        Brush brush3 = new SolidBrush(Color.White);
        Brush brush4 = new SolidBrush(Color.Black);

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Ticker Feed";

            if (marketData == null) return;

            Font font = this.Font;
            Pen p1 = new Pen(brush3);

            g.Clear(this.BackColor);

            string[,] lines = new string[marketData.Length, 4];
            for (int i = 0; i < marketData.Length; i++) {
                lines[i, 0] = marketData[i].pair.ToString().Replace("_", " / ");
                lines[i, 1] = marketData[i].data.PriceLast.ToString("F8").Trim();
                lines[i, 2] = marketData[i].data.Volume24HourBase.ToString("F3").Trim();
                lines[i, 3] = (marketData[i].data.PriceChangePercentage * 100f).ToString("F2") + "%";
            }

            float posX, posY;
            float height = font.Height * 1.25f;

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;

            // now draw the tickers

            maxScrollYOffset = ((marketData.Length + 1) * FontHeight * 1.25f) - Height;

            for (int i = 0; i < marketData.Length; i++) {
                posY = ((i + 1) * font.Height) * 1.25f - (scrollYOffset * maxScrollYOffset);
                posX = 5.0f;

                if (i % 2 == 0) g.FillRectangle(backBrush1, 0, posY - 2, 400, height);

                float changePaintFactor = (float)marketData[i].data.PriceChangePercentage * 5f;
                if (changePaintFactor < -1) changePaintFactor = -1;
                if (changePaintFactor > 1) changePaintFactor = 1;

                int diff = (int)(changePaintFactor * 120);
                brush2 = new SolidBrush(Color.FromArgb(128 - diff, 128 + diff, 128));

                Brush nameBrush = brush1;
                if (TradedPairs.ContainsKey(marketData[i].pair)) nameBrush = brush1Emphasis;
                

                g.DrawString(lines[i, 0], font, nameBrush, posX, posY);

                posX = 110.0f;
                g.DrawString(lines[i, 1], font, brush1, posX, posY);

                posX = 300.0f;
                g.DrawString(lines[i, 2], font, brush1, posX, posY, format);

                posX = 380.0f;
                g.DrawString(lines[i, 3], font, brush2, posX, posY, format);

            }

            // draw header

            g.FillRectangle(brush4, 0, 0, 400, font.Height * 1.25f);

            g.DrawString("Coin", font, brush1, 15, 2);
            g.DrawString("Price", font, brush1, 125, 2);
            g.DrawString("Volume", font, brush1, 287, 2, format);
            g.DrawString("Change", font, brush1, 380, 2, format);

            g.DrawString("", font, brush1, 5, 5, StringFormat.GenericTypographic);

            // draw flashing change markers

            for (int i = 0; i < marketData.Length; i++) {
                posY = ((i + 1) * font.Height) * 1.25f - (scrollYOffset * maxScrollYOffset);
                if (lastUpdateChange[i] > 0) {
                    int colorDiff = lastUpdateDir[i] == 0 ? 0 : (lastUpdateDir[i] > 0 ? 120 : -120);
                    g.DrawRectangle(new Pen(Color.FromArgb((int)(lastUpdateChange[i] * 255), 128 - colorDiff, 128 + colorDiff, 128)), 0,
                        posY - 1, 399, (font.Height * 1.25f) - 1);
                    lastUpdateChange[i] -= 0.05f;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoloniexAPI;
using PoloniexAPI.MarketTools;

namespace PoloniexBot.Windows.Controls {
    public partial class TradeHistoryScreen : MultiThreadControl {
        public TradeHistoryScreen () {
            InitializeComponent();
            locker = new object();
        }

        private static Object locker;

        Utility.TradeTracker.TradeData[] Trades;
        Utility.TradeTracker.TradeData[] DoneTrades;

        public void UpdateTrades (Utility.TradeTracker.TradeData[] buyTrades, Utility.TradeTracker.TradeData[] doneTrades) {
            lock (locker) {
                this.Trades = buyTrades;
                this.DoneTrades = doneTrades;
                RecalculateCumulative();
            }
        }

        public void RecalculateCumulative () {
            
            double cumulative = 0;
            if (this.DoneTrades != null) {
                for (int i = 0; i < DoneTrades.Length; i++) {
                    if (i == 0) DoneTrades[0].cumulativeNetGainBtc = DoneTrades[0].netGainBtc;
                    else DoneTrades[i].cumulativeNetGainBtc = DoneTrades[i - 1].cumulativeNetGainBtc + DoneTrades[i].netGainBtc;
                    cumulative = DoneTrades[i].cumulativeNetGainBtc;
                }
            }

            for (int i = 0; i < Trades.Length; i++) {
                if (i == 0) Trades[i].cumulativeNetGainBtc = Trades[i].netGainBtc + cumulative;
                else Trades[i].cumulativeNetGainBtc = Trades[i - 1].cumulativeNetGainBtc + Trades[i].netGainBtc;
            }
        }

        // -----------------------------------------------------------

        public MarketPeriod period = MarketPeriod.Minutes30;

        Brush brushBorder = new SolidBrush(Color.FromArgb(64, 64, 64));
        Brush brushBuy = new SolidBrush(Color.FromArgb(0, 32, 0));
        Brush brushSell = new SolidBrush(Color.FromArgb(32, 0, 0));
        Brush brushOpen = new SolidBrush(Color.FromArgb(32, 32, 0));

        Brush brushText = new SolidBrush(Color.Gray);
        Brush brushTextEmphasis = new SolidBrush(Color.Silver);
        Pen PenTextOutline = new Pen(new SolidBrush(Color.FromArgb(16, 16, 16)), 5);

        Font fontSmall = new System.Drawing.Font("Calibri Caps", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        Font fontSmallBold = new System.Drawing.Font("Calibri Bold Caps", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        Font fontMedium = new System.Drawing.Font("Calibri Bold Caps", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        Font fontLarge = new System.Drawing.Font("Calibri Bold Caps", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

        private const int TradeBoxWidth = 100;

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Trade History";

            lock (locker) {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(this.BackColor);

                float leftBorder = 5;
                float rightBorder = Width - 5;

                float topBorder = 5;
                float bottomBorder = Height - 5;

                int posXIndex = 0;
                int posYStart = 41;

                if (Trades != null && Trades.Length > 0) {
                    for (int i = Trades.Length - 1; i >= 0; i--) {

                        int posX = (int)(rightBorder - ((TradeBoxWidth + 5) * (1 + posXIndex)));

                        if (posX + TradeBoxWidth > rightBorder) continue;
                        if (posX < leftBorder) break;

                        DrawTrade(g, Trades[i], new Rectangle(posX, posYStart, TradeBoxWidth, 68), true);
                        DrawOpenPosition(g, Trades[i], new Rectangle(posX, posYStart + 68 + 5, TradeBoxWidth, 48));

                        posXIndex++;
                    }
                }

                if (DoneTrades != null && DoneTrades.Length > 0) {
                    for (int i = DoneTrades.Length - 1; i >= 0; i--) {

                        int posX = (int)(rightBorder - ((TradeBoxWidth + 5) * (1 + posXIndex)));

                        if (posX + TradeBoxWidth > rightBorder) continue;
                        if (posX < leftBorder) break;

                        DrawTrade(g, DoneTrades[i], new Rectangle(posX, posYStart, TradeBoxWidth, 68), true);

                        DrawTrade(g, DoneTrades[i], new Rectangle(posX, posYStart + 68 + 5, TradeBoxWidth, 48), false);
                        DrawNetTrade(g, DoneTrades[i], new Rectangle(posX, posYStart + 68 + 48 + 10, TradeBoxWidth, 36));
                        DrawCumulativeNet(g, DoneTrades[i], new Rectangle(posX, posYStart + 68 + 48 + 36 + 15, TradeBoxWidth, 36));

                        posXIndex++;
                    }
                }

                // todo: scrollbar
                // just a persistent float that gets added to posX

                g.DrawString("Trade History", fontLarge, brushTextEmphasis, leftBorder + 10, topBorder + 5);
            }
        }

        private void DrawCumulativeNet (Graphics g, Utility.TradeTracker.TradeData trade, Rectangle rect) {
            
            bool isProfit = trade.cumulativeNetGainBtc > 0;

            // draw background
            g.FillRectangle(isProfit ? brushBuy : brushSell, rect);

            float width;
            float posY = rect.Y + 5;

            using (Pen pen = new Pen(brushBorder, 1)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width, rect.Y);
                g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
            }

            // draw title
            string title = "Cumulative Gain";
            width = g.MeasureString(title, fontSmall).Width;
            float textPos = rect.X + (rect.Width / 2f) - (width / 2f);
            g.DrawString(title, fontSmall, brushText, textPos, posY);

            posY += fontSmall.Height;

            // draw net gain in BTC
            double netGain = trade.cumulativeNetGainBtc;
            string net = (netGain > 0 ? "+" : "") + netGain.ToString("F8") + " BTC";

            width = g.MeasureString(net, fontSmall).Width;
            textPos = rect.X + (rect.Width / 2f) - (width / 2f);
            g.DrawString(net, fontSmall, brushText, textPos, posY);
        }

        private void DrawNetTrade (Graphics g, Utility.TradeTracker.TradeData trade, Rectangle rect) {

            bool isProfit = trade.percentGain > 0;

            // draw background
            g.FillRectangle(isProfit ? brushBuy : brushSell, rect);

            float width;
            float posY = rect.Y + 5;

            using (Pen pen = new Pen(brushBorder, 1)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width, rect.Y);
                g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
            }

            // draw profit percent
            string percentNet = isProfit ? "Profit: +" : "Loss: ";
            percentNet += trade.percentGain.ToString("F4") + "%";

            width = g.MeasureString(percentNet, fontSmall).Width;
            float textPos = rect.X + (rect.Width / 2f) - (width / 2f);
            g.DrawString(percentNet, fontSmall, brushText, textPos, posY);
            
            posY += fontSmall.Height;

            // draw net gain in BTC
            double netGain = trade.netGainBtc;
            string net = (netGain > 0 ? "+" : "") + netGain.ToString("F8") + " BTC";

            width = g.MeasureString(net, fontSmall).Width;
            textPos = rect.X + (rect.Width / 2f) - (width / 2f);
            g.DrawString(net, fontSmall, brushText, textPos, posY);
        }
        private void DrawTrade (Graphics g, Utility.TradeTracker.TradeData trade, Rectangle rect, bool isBuy) {

            // draw background
            // g.FillRectangle(isBuy ? brushBuy : brushSell, rect);

            float width;
            float posY = 5;

            if (isBuy) {
                // draw quote currency name
                width = g.MeasureString(trade.pair.QuoteCurrency, fontMedium).Width;
                float textPos = rect.X + (rect.Width / 2f) - (width / 2f);
                g.DrawString(trade.pair.QuoteCurrency, fontMedium, brushText, textPos, rect.Y + 3);
                posY += fontMedium.Height;
            }

            // draw borders
            using (Pen pen = new Pen(brushBorder, 1)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width, rect.Y);
                g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
            }

            // draw date + time
            DateTime dt = Utility.DateTimeHelper.UnixTimestampToDateTime(isBuy ? trade.buyTimestamp : trade.sellTimestamp);
            string dateTime = dt.TimeOfDay.ToString() + " (" + dt.Day + "." + dt.Month + ")";
            width = g.MeasureString(dateTime, fontSmall).Width;
            g.DrawString(dateTime, fontSmall, brushText, rect.X - (width / 2f) + (rect.Width / 2f), rect.Y + posY);

            posY += fontSmall.Height;

            // draw price
            double buyPrice = isBuy ? trade.buyPrice : trade.sellPrice;
            string price = "Price: " + buyPrice.ToString("F8");
            width = g.MeasureString(price, fontSmall).Width;
            g.DrawString(price, fontSmall, brushText, rect.X - (width / 2f) + (rect.Width / 2f), rect.Y + posY);

            posY += fontSmall.Height;

            // draw amount in btc
            double amountBtc = isBuy ? (trade.buyAmountQuote * trade.buyPrice) : (trade.sellAmountQuote * trade.sellPrice);
            string amount = amountBtc.ToString("F8") + " BTC";
            width = g.MeasureString(amount, fontSmall).Width;
            g.DrawString(amount, fontSmall, brushText, rect.X - (width / 2f) + (rect.Width / 2f), rect.Y + posY);

        }
        private void DrawOpenPosition (Graphics g, Utility.TradeTracker.TradeData trade, Rectangle rect) {

            double change = ((trade.openPrice - trade.buyPrice) / trade.buyPrice);
            double gain = trade.buyAmountQuote * change * trade.openPrice;
            change *= 100;
            
            // draw background
            // g.FillRectangle(brushOpen, rect);

            float width;
            float posY = 5;

            // draw borders
            using (Pen pen = new Pen(brushBorder, 1)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width, rect.Y);
                g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
            }

            // draw price
            string price = "Price: " + trade.openPrice.ToString("F8");
            width = g.MeasureString(price, fontSmall).Width;
            g.DrawString(price, fontSmall, brushText, rect.X - (width / 2f) + (rect.Width / 2f), rect.Y + posY);

            posY += fontSmall.Height;

            // draw change percent
            using (Brush changeBrush = new SolidBrush(GetChangeColor(change))) {
                string changeString = (change > 0 ? "+" : "") + change.ToString("F3") + "%";
                width = g.MeasureString(changeString, fontSmall).Width;
                float posX = (rect.Width / 2f) - (width / 2f) + 5;
                g.DrawString(changeString, fontSmall, changeBrush, rect.X + posX, rect.Y + posY);
            }

            posY += fontSmall.Height;

            // draw gain
            string gainString = gain.ToString("F8") + " BTC";
            width = g.MeasureString(gainString, fontSmall).Width;
            g.DrawString(gainString, fontSmall, brushText, rect.X - (width / 2f) + (rect.Width / 2f), rect.Y + posY);
        }

        private Color GetChangeColor (double value) {

            // +4% = 0,255,0
            // 0 = 255,255,255
            // -4% = 255, 0, 0

            int r, g, b;
            r = g = b = 255;

            value /= 4;
            int offset = (int)(63 * value);
            offset = offset > 63 ? 63 : (offset < -63 ? -63 : offset);

            r = 164 - offset;
            g = 164 + offset;
            b = 164 - Math.Abs(offset);

            return Color.FromArgb(r, g, b);
        }
    }
}

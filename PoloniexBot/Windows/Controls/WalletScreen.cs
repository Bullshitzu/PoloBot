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
    public partial class WalletScreen : Label {
        public WalletScreen () {
            InitializeComponent();
        }

        struct AvailableAmount : IComparable<AvailableAmount> {
            public string currency;
            public double amount;
            public double inOrders;
            public double btcValue;
            
            public AvailableAmount (string currency, double amount, double inOrders, double btcValue) {
                this.currency = currency;
                this.amount = amount;
                this.inOrders = inOrders;
                this.btcValue = btcValue;
            }

            public int CompareTo (AvailableAmount other) {
                return other.btcValue.CompareTo(btcValue);
            }
        }

        List<AvailableAmount> AvailableAmounts;
        double TotalBTCValue;
        int orderCount = 0;

        public void UpdateBalance (IDictionary<string, PoloniexAPI.WalletTools.IBalance> wallet) {
            if (wallet == null) return;
            if (AvailableAmounts == null) AvailableAmounts = new List<AvailableAmount>();

            lock (AvailableAmounts) {
                KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] list = wallet.ToArray();
                AvailableAmounts.Clear();
                TotalBTCValue = 0;

                for (int i = 0; i < list.Length; i++) {
                    if (list[i].Value.QuoteAvailable > 0)
                        AvailableAmounts.Add(new AvailableAmount(list[i].Key, list[i].Value.QuoteAvailable, list[i].Value.QuoteOnOrders, list[i].Value.BitcoinValue));
                    TotalBTCValue += list[i].Value.BitcoinValue;
                }

                AvailableAmounts.Sort();
            }

            Invalidate();
        }

        public void UpdateOrderCount (int orderCount) {
            this.orderCount = orderCount;
        }

        protected override void OnPaint (PaintEventArgs e) {

            try {

                Graphics g = e.Graphics;
                Font font = Font;
                Font largeFont = new System.Drawing.Font(
                    "Calibri Bold Caps", 16F,
                    System.Drawing.FontStyle.Bold,
                    System.Drawing.GraphicsUnit.Point,
                    ((byte)(238)));
                Brush brush = new SolidBrush(Color.Gray);
                Brush brushEmphasis = new SolidBrush(Color.Silver);

                System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
                nfi.NumberGroupSeparator = " ";
                nfi.NumberDecimalSeparator = ".";

                g.DrawString("Wallet", largeFont, brushEmphasis, 10, 10);

                float posX = 15;
                float posY = 50;

                g.DrawString("API Key: ", font, brush, posX, posY);
                posX += g.MeasureString("API Key: ", font).Width;
                string key = PoloniexBot.ClientManager.client.Authenticator.PublicKey;
                g.DrawString(key.Split('-')[0] + " - Trade", font, brush, posX, posY);

                posX = 15;
                posY += font.Height * 1.2f;

                g.DrawString("BTC Value:", font, brush, posX, posY);
                posX += g.MeasureString("BTC Value: ", font).Width;
                g.DrawString(TotalBTCValue.ToString("F8", nfi), font, brushEmphasis, posX, posY);

                posX = 15;
                posY += font.Height * 1.5f;

                g.DrawString("Active Orders:", font, brush, posX, posY);
                posX += g.MeasureString("Active Orders: ", font).Width;
                g.DrawString(orderCount.ToString(), font, brushEmphasis, posX, posY);

                // ------------

                Pen pen = new Pen(brush, 1);
                g.DrawLine(pen, new Point(240, 50), new Point(240, 255));

                posX = 250;
                posY = 15;

                g.DrawString("Available:", font, brushEmphasis, posX, posY);

                posY = 50;

                if (AvailableAmounts == null) return;
                lock (AvailableAmounts) {
                    for (int i = 0; i < AvailableAmounts.Count && i < 9; i++) {

                        g.DrawString(AvailableAmounts[i].currency, font, brush, posX, posY);

                        string btcValue = AvailableAmounts[i].btcValue.ToString("F6", nfi) + " BTC";
                        float width = g.MeasureString(btcValue, font).Width;

                        g.DrawString(btcValue, font, brush, 485 - width, posY);

                        posX = 250;
                        posY += font.Height * 1.1f;

                        if (i == 8 && AvailableAmounts.Count > 10) {
                            g.DrawString(". . .", font, brush, 350, posY);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine("WALLET EXCEPTION: " + ex.Message + "\n" + ex.StackTrace);
            }

        }
    }
}

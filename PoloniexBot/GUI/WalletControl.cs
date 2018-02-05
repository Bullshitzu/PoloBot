using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.GUI {
    public class WalletControl : Templates.BaseControl {

        public struct Balance {

            public string quoteName;
            public double quoteAmount;
            public double btcValue;

            public Balance (string quoteName, double quoteAmount, double btcValue) {
                this.quoteName = quoteName;
                this.quoteAmount = quoteAmount;
                this.btcValue = btcValue;
            }

        }

        public KeyValuePair<string, PoloniexAPI.WalletTools.IBalance>[] balances = null;

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            try {
                if (balances != null) {

                    double btcValue = 0;
                    for (int i = 0; i < balances.Length; i++) {
                        if (balances[i].Key == "BTC") btcValue += balances[i].Value.QuoteAvailable;
                        else btcValue += balances[i].Value.BitcoinValue;
                    }

                    // title + BTC value

                    string text = "Wallet:" + btcValue.ToString("F8") + "BTC";
                    float width = g.MeasureString(text, Style.Fonts.Title).Width;

                    float posX = (Width / 2) - ((width + 7) / 2);

                    text = "Wallet:";
                    width = g.MeasureString(text, Style.Fonts.Title).Width;

                    string[] btcValueSplit = Helper.SplitLeadingZeros(btcValue.ToString("F8"));

                    float posY = 10;

                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        g.DrawString(text, Style.Fonts.Title, brush, posX, posY);
                    }

                    using (Brush brushDark = new SolidBrush(Style.Colors.Terciary.Dark2)) {
                        using (Brush brushNormal = new SolidBrush(Style.Colors.Terciary.Main)) {

                            g.DrawString(btcValueSplit[0], Style.Fonts.Medium, brushDark, posX + width, posY);
                            width += g.MeasureString(btcValueSplit[0], Style.Fonts.Medium).Width;

                            g.DrawString(btcValueSplit[1], Style.Fonts.Medium, brushNormal, posX + width - 3, posY);

                        }
                    }

                    width += g.MeasureString(btcValueSplit[1], Style.Fonts.Medium).Width;
                    text = "BTC";

                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        g.DrawString(text, Style.Fonts.Title, brush, posX + width, posY);
                    }

                    posY = 35;

                    // USD worth

                    double usdWorth = btcValue * Trading.Strategies.BaseTrendMonitor.LastUSDTBTCPrice;
                    string usdWorthText = usdWorth > 0 ? usdWorth.ToString("F2") : "???";

                    text = "$ " + usdWorthText;
                    width = g.MeasureString(text, Style.Fonts.Title).Width;

                    posX = (Width / 2) - ((width + 7) / 2);

                    text = "$ ";
                    width = g.MeasureString(text, Style.Fonts.Title).Width;

                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        g.DrawString(text, Style.Fonts.Title, brush, posX, posY);
                    }

                    using (Brush brushDark = new SolidBrush(Style.Colors.Terciary.Dark2)) {
                        g.DrawString(usdWorthText, Style.Fonts.Medium, brushDark, posX + width, posY);       
                    }

                    posY = 70;

                    // available funds

                    text = "Available";
                    width = g.MeasureString(text, Style.Fonts.Small).Width;

                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        g.DrawString(text, Style.Fonts.Small, brush, (Width / 2) - (width / 2), posY);
                    }

                    using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                        using (Brush brushDark = new SolidBrush(Style.Colors.Primary.Dark2)) {
                            using (Pen pen = new Pen(Style.Colors.Primary.Dark1)) {

                                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                                int cnt = 0;

                                for (int i = 0; i < balances.Length; i++) {
                                    if (balances[i].Value.BitcoinValue < 0.00000001) continue;

                                    posY = 95 + (cnt * 25);
                                    cnt++;

                                    // quote name

                                    g.DrawString(balances[i].Key, Style.Fonts.Reduced, brush, 7, posY);

                                    // quote amount

                                    int digitCnt = GetDecimalCount((int)balances[i].Value.QuoteAvailable);

                                    string[] parts = Helper.SplitLeadingZeros(balances[i].Value.QuoteAvailable.ToString("F" + digitCnt));
                                    width = g.MeasureString(parts[0], Style.Fonts.Reduced).Width;

                                    int partSpacing = parts[0] == "" ? 0 : -4;

                                    g.DrawString(parts[0], Style.Fonts.Reduced, brushDark, Width - 240, posY);
                                    g.DrawString(parts[1], Style.Fonts.Reduced, brush, Width - 240 + width + partSpacing, posY);


                                    // btc value

                                    btcValue = 0;
                                    if (balances[i].Key == "BTC") btcValue = balances[i].Value.QuoteAvailable;
                                    else btcValue = balances[i].Value.BitcoinValue;

                                    digitCnt = GetDecimalCount((int)btcValue);

                                    parts = Helper.SplitLeadingZeros(btcValue.ToString("F" + digitCnt));
                                    width = g.MeasureString(parts[0], Style.Fonts.Reduced).Width;

                                    partSpacing = parts[0] == "" ? 0 : -4;

                                    g.DrawString(parts[0], Style.Fonts.Reduced, brushDark, Width - 125, posY);
                                    g.DrawString(parts[1], Style.Fonts.Reduced, brush, Width - 125 + width + partSpacing, posY);

                                    width += g.MeasureString(parts[1], Style.Fonts.Reduced).Width;

                                    g.DrawString("BTC", Style.Fonts.Reduced, brushDark, Width - 125 + width - 2, posY);

                                    if (i + 1 < balances.Length) g.DrawLine(pen, 10, posY + 20, Width - 10, posY + 20);

                                }
                            }
                        }
                    }
                }
                else DrawNoData(g);

                // Draw borders
                DrawBorders(g);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message + " - " + ex.StackTrace);
            }
        }

        private int GetDecimalCount (int val) {
            if (val < 1) return 8;

            int log = (int)Math.Log10(val);
            // log = number of digits until decimal point

            int cnt = 8 - log;

            if (cnt < 0) cnt = 0;
            if (cnt > 8) cnt = 8;

            return cnt;
        }
    }
}

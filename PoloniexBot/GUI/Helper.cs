using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoloniexBot.GUI {
    class Helper {

        public static void DrawThreadIcon (Graphics g, PointF point, Color color) {
            using (Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, point.X, point.Y, 15, 15);
                g.DrawEllipse(pen, point.X + 5, point.Y + 5, 5, 5);
            }
        }

        public static void DrawGraphLine (Graphics g, RectangleF rect, double[] points, double max, Color lineColor, Color pointColor) {
            DrawGraphLine(g, rect, points, max, 0, lineColor, pointColor);
        }
        public static void DrawGraphLine (Graphics g, RectangleF rect, double[] points, double max, double min, Color lineColor, Color pointColor, float pointRadius = 2.5f) {
            if (points != null) {

                float lastPosX = -5;
                float lastPosY = -5;

                using (Brush brush = new SolidBrush(pointColor)) {
                    using (Pen pen = new Pen(lineColor, 2)) {
                        for (int i = 0; i < points.Length; i++) {

                            float posX = ((i / (float)(points.Length - 1)) * rect.Width) + rect.X;
                            float posY = (float)(((max - points[i]) / (max - min)) * rect.Height + rect.Y);

                            if (lastPosX > 0 && lastPosY > 0) {
                                g.DrawLine(pen, lastPosX, lastPosY, posX, posY);
                                g.FillEllipse(brush, lastPosX - pointRadius, lastPosY - pointRadius, pointRadius * 2, pointRadius * 2);
                            }

                            lastPosX = posX;
                            lastPosY = posY;
                        }

                        g.FillEllipse(brush, lastPosX - pointRadius, lastPosY - pointRadius, pointRadius * 2, pointRadius * 2);
                    }
                }
            }
        }
        public static void DrawGraphLine (Graphics g, RectangleF rect, PoloniexAPI.TickerChangedEventArgs[] tickers, double max, double min, long startTime, int pointCount, float pointRadius) {
            if (tickers == null) return;

            float lastPosX = -5;
            float lastPosYBuy = -5;
            float lastPosYSell = -5;

            long nextValidPoint = startTime;
            int dTime = (int)(tickers.Last().Timestamp - startTime);
            
            using (Brush brushBuy = new SolidBrush(Style.Colors.Secondary.Light1)) {
                using (Pen penBuy = new Pen(Style.Colors.Secondary.Dark1, 2)) {
                    using (Brush brushSell = new SolidBrush(Style.Colors.Terciary.Main)) {
                        using (Pen penSell = new Pen(Style.Colors.Terciary.Dark2, 2)) {

                            double currMin = double.MaxValue;
                            double currMax = double.MinValue;

                            long firstValidTimestamp = startTime - (dTime / pointCount);

                            for (int i = 0; i < tickers.Length; i++) {
                                if (tickers[i].Timestamp < firstValidTimestamp) continue;

                                if (tickers[i].MarketData.OrderTopBuy < currMin) currMin = tickers[i].MarketData.OrderTopBuy;
                                if (tickers[i].MarketData.OrderTopSell > currMax) currMax = tickers[i].MarketData.OrderTopSell;

                                if (tickers[i].Timestamp >= nextValidPoint) { //  || i + 1 >= tickers.Length

                                    float posX = (((float)(nextValidPoint - startTime) / dTime) * rect.Width) + rect.X - 1;
                                    if (i + 1 >= tickers.Length) posX = (((float)(tickers.Last().Timestamp - startTime) / dTime) * rect.Width) + rect.X - 1;
                                    if (i == 0) posX = rect.X;

                                    float posYBuy = (float)(((max - currMin) / (max - min)) * rect.Height + rect.Y);
                                    float posYSell = (float)(((max - currMax) / (max - min)) * rect.Height + rect.Y);

                                    if (lastPosX > 0 && lastPosYBuy > 0) {
                                        g.DrawLine(penBuy, lastPosX, lastPosYBuy, posX, posYBuy);
                                        g.FillEllipse(brushBuy, lastPosX - pointRadius, lastPosYBuy - pointRadius, pointRadius * 2, pointRadius * 2);
                                    }
                                    if (lastPosX > 0 && lastPosYSell > 0) {
                                        g.DrawLine(penSell, lastPosX, lastPosYSell, posX, posYSell);
                                        g.FillEllipse(brushSell, lastPosX - pointRadius, lastPosYSell - pointRadius, pointRadius * 2, pointRadius * 2);
                                    }

                                    lastPosX = posX;
                                    lastPosYBuy = posYBuy;
                                    lastPosYSell = posYSell;

                                    nextValidPoint += dTime / pointCount;

                                    currMin = double.MaxValue;
                                    currMax = double.MinValue;
                                }
                            }

                            float finalPosX = (((float)(tickers.Last().Timestamp - startTime) / dTime) * rect.Width) + rect.X - 1;

                            float finalPosYBuy = (float)(((max - currMin) / (max - min)) * rect.Height + rect.Y);
                            float finalPosYSell = (float)(((max - currMax) / (max - min)) * rect.Height + rect.Y);

                            g.FillEllipse(brushBuy, finalPosX - pointRadius, lastPosYBuy - pointRadius, pointRadius * 2, pointRadius * 2);
                            g.FillEllipse(brushSell, finalPosX - pointRadius, lastPosYSell - pointRadius, pointRadius * 2, pointRadius * 2);

                            // draw triangle indicating current price

                            double currPrice = tickers.Last().MarketData.PriceLast;
                            float posY = (float)(((max - currPrice) / (max - min)) * rect.Height + rect.Y);

                            using (Pen penTriangle = new Pen(Style.Colors.Primary.Light1, 2)) {
                                g.DrawLine(penTriangle, finalPosX + 4, posY, finalPosX + 7, posY - 3);
                                g.DrawLine(penTriangle, finalPosX + 4, posY, finalPosX + 7, posY + 3);
                            }
                        }
                    }
                }
            }
        }

        public static void DrawRectangleFancy1 (Graphics g, RectangleF rect, Color color, float thickness) {

            int size = 5;

            using (Pen pen = new Pen(color, thickness)) {

                g.DrawLine(pen, rect.X, rect.Y, rect.X + size, rect.Y);
                g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Y + size);

                g.DrawLine(pen, rect.X, rect.Y + rect.Height, rect.X + size, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X, rect.Y + rect.Height, rect.X, rect.Y + rect.Height - size);

                g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X + rect.Width - size, rect.Y);
                g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + size);

                g.DrawLine(pen, rect.X + rect.Width, rect.Y + rect.Height, rect.X + rect.Width - size, rect.Y + rect.Height);
                g.DrawLine(pen, rect.X + rect.Width, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height - size);
            }
        }

        public static void DrawTextShadow (Graphics g, string text, PointF point, Font font, Color color) {

            point = new PointF(point.X, point.Y - 0.5f);

            GraphicsPath path = new GraphicsPath();
            path.AddString(text, font.FontFamily, (int)font.Style, g.DpiY * font.SizeInPoints / 72, point, StringFormat.GenericDefault);

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            System.Drawing.Drawing2D.InterpolationMode oldInterpolationMode = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.Bilinear;

            using (Pen pen = new Pen(color, 5)) {
                g.DrawPath(pen, path);
            }


            g.SmoothingMode = oldSmoothingMode;
            g.InterpolationMode = oldInterpolationMode;
        }

        public static string[] SplitLeadingZeros (string number) {

            string p1 = "";
            string p2 = "";

            bool zerosEnded = false;
            for (int i = 0; i < number.Length; i++) {
                if (zerosEnded) {
                    p2 += number[i];
                }
                else {
                    char currChar = number[i];
                    if (currChar == '0' || currChar == '.' || currChar == ',') p1 += currChar;
                    else {
                        zerosEnded = true;
                        p2 += currChar;
                    }
                }
            }

            return new string[] { p1, p2 };
        }

        public static Color MultiplyColor (Color baseColor, float value, bool multAlpha = false) {

            float r = baseColor.R / 255f;
            float g = baseColor.G / 255f;
            float b = baseColor.B / 255f;

            r *= value;
            g *= value;
            b *= value;

            r *= 255;
            g *= 255;
            b *= 255;

            r = r < 0 ? 0 : (r > 255 ? 255 : r);
            g = g < 0 ? 0 : (g > 255 ? 255 : g);
            b = b < 0 ? 0 : (b > 255 ? 255 : b);

            float a = 255;
            if (multAlpha) {
                a = baseColor.A / 255f;
                a *= value;
                a *= 255;
                a = a < 0 ? 0 : (a > 255 ? 255 : a);
            }

            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        public static Color LerpColor (float t, Color minColor, Color maxColor) {

            int r = (int)Lerp(t, minColor.R, maxColor.R);
            int g = (int)Lerp(t, minColor.G, maxColor.G);
            int b = (int)Lerp(t, minColor.B, maxColor.B);

            r = r < 0 ? 0 : (r > 255 ? 255 : r);
            g = g < 0 ? 0 : (g > 255 ? 255 : g);
            b = b < 0 ? 0 : (b > 255 ? 255 : b);

            return Color.FromArgb(r, g, b);
        }
        public static Color LerpColor (float val, float min, float max, Color minColor, Color maxColor) {

            if (max < min) {
                float temp = max;
                max = min;
                min = temp;
                val = 1 - val;
            }

            // turn val into a value between 0 and 1
            float normalized = (val - min) / (max - min);

            return LerpColor(normalized, minColor, maxColor);
        }

        public static float Lerp (float val, float min, float max) {
            if (max < min) {
                float temp = max;
                max = min;
                min = temp;
                val = 1 - val;
            }

            return (max - min) * val + min;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Utility {
    public static class DrawingHelper {

        public static void DrawLine (Graphics g, PointF p1, PointF p2, Color color) {
            DrawLine(g, p1, p2, color, 1);
        }
        public static void DrawLine (Graphics g, PointF p1, PointF p2, Color color, float thickness) {
            using (Brush brush = new SolidBrush(color)) {
                using (Pen pen = new Pen(brush, thickness)) {
                    g.DrawLine(pen, p1, p2);
                }
            }
        }

        public static void DrawDashedLine (Graphics g, PointF p1, PointF p2, Color color) {
            DrawDashedLine(g, p1, p2, color, 1);
        }
        public static void DrawDashedLine (Graphics g, PointF p1, PointF p2, Color color, float thickness) {
            using (Brush brush = new SolidBrush(color)) {
                using (Pen pen = new Pen(brush, thickness)) {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawLine(pen, p1, p2);
                }
            }
        }

        public static void DrawGradientLine (Graphics g, PointF p1, PointF p2, Color col1, Color col2) {
            DrawGradientLine(g, p1, p2, col1, col2, 1);
        }
        public static void DrawGradientLine (Graphics g, PointF p1, PointF p2, Color col1, Color col2, float thickness) {
            if (((int)p1.X) == ((int)p2.X) && ((int)p1.Y) == ((int)p2.Y)) return;
            using (LinearGradientBrush brush = new LinearGradientBrush(p1, p2, col1, col2)) {
                using (Pen pen = new Pen(brush, thickness)) {
                    g.DrawLine(pen, p1, p2);
                }
            }
        }

        public static void DrawBorders (Graphics g, PointF topLeft, PointF bottomRight) {
            using (Brush brush = new SolidBrush(Color.FromArgb(107, 144, 148))) {
                using (Pen pen = new Pen(brush, 2)) {
                    // horizontal
                    g.DrawLine(pen, topLeft, new PointF(bottomRight.X, topLeft.Y));
                    g.DrawLine(pen, new PointF(topLeft.X, bottomRight.Y), bottomRight);
                    // vertical
                    g.DrawLine(pen, topLeft, new PointF(topLeft.X, bottomRight.Y));
                    g.DrawLine(pen, new PointF(bottomRight.X, topLeft.Y), bottomRight);
                }
            }
        }
        public static void DrawGrid (Graphics g, PointF topLeft, PointF bottomRight, int horCount, int verCount) {
            using (Brush brush = new SolidBrush(Color.FromArgb(23, 42, 44))) {
                using (Pen pen = new Pen(brush, 1)) {

                    float xSize = (bottomRight.X - topLeft.X) / horCount;
                    float ySize = (bottomRight.Y - topLeft.Y) / verCount;

                    for (int i = 1; i < horCount; i++) {
                        float xPos = topLeft.X + (xSize * i);
                        g.DrawLine(pen, new PointF(xPos, topLeft.Y), new PointF(xPos, bottomRight.Y));
                    }
                    for (int i = 1; i < verCount; i++) {
                        float yPos = topLeft.Y + (ySize * i);
                        g.DrawLine(pen, new PointF(topLeft.X, yPos), new PointF(bottomRight.X, yPos));
                    }
                }
            }
        }

        public static void DrawShadow (Graphics g, RectangleF rect, Color color) {

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(color)) {
                using (Pen pen = new Pen(Color.Black, 3)) {
                    using (System.Drawing.Drawing2D.GraphicsPath path = new GraphicsPath()) {
                        path.AddRectangle(rect);
                        g.DrawPath(pen, path);
                        g.FillPath(brush, path);
                    }
                }
            }

            g.SmoothingMode = oldSmoothingMode;

        }
        public static void DrawCircle (Graphics g, float xPos, float yPos, float radius, Color color) {

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(color)) {
                g.FillEllipse(brush, xPos - radius, yPos - radius, radius * 2, radius * 2);
            }

            g.SmoothingMode = oldSmoothingMode;
        }
        public static void DrawShadow (Graphics g, string text, Font font, Color color, float xPos, float yPos) {

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(color)) {
                using (Pen pen = new Pen(Color.Black, 3)) {
                    using (System.Drawing.Drawing2D.GraphicsPath path = GetStringPath(text, g.DpiY, font, new PointF(xPos, yPos))) {
                        g.DrawPath(pen, path);
                        g.FillPath(brush, path);
                    }
                }
            }

            g.SmoothingMode = oldSmoothingMode;
        }
        private static System.Drawing.Drawing2D.GraphicsPath GetStringPath (string text, float dpi, Font font, PointF point) {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            float emSize = font.Size * 1.38f;
            path.AddString(text, font.FontFamily, (int)font.Style, emSize, point, StringFormat.GenericTypographic);
            return path;
        }

        public static Color BlendHistogramColor (Color mainColor, Color defaultColor, double currValue, double maxValue) {

            // 15%
            double val = (maxValue - System.Math.Abs(currValue)) / maxValue;
            val = 1 - val;
            val *= 3; // 1/15 (15%=max)

            if (val > 1) val = 1;
            if (val < 0) val = 0;

            int aDiff = mainColor.A - defaultColor.A;
            int rDiff = mainColor.R - defaultColor.R;
            int gDiff = mainColor.G - defaultColor.G;
            int bDiff = mainColor.B - defaultColor.B;

            aDiff = (int)(aDiff * val);
            rDiff = (int)(rDiff * val);
            gDiff = (int)(gDiff * val);
            bDiff = (int)(bDiff * val);

            aDiff += defaultColor.A;
            rDiff += defaultColor.R;
            gDiff += defaultColor.G;
            bDiff += defaultColor.B;

            if (aDiff < 0) aDiff = 0;
            if (aDiff > 255) aDiff = 255;
            if (rDiff < 0) rDiff = 0;
            if (rDiff > 255) rDiff = 255;
            if (gDiff < 0) gDiff = 0;
            if (gDiff > 255) gDiff = 255;
            if (bDiff < 0) bDiff = 0;
            if (bDiff > 255) bDiff = 255;

            return Color.FromArgb(aDiff, rDiff, gDiff, bDiff);
        }
    }
}

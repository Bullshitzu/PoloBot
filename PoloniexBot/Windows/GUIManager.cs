using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Utility;

namespace PoloniexBot.Windows {
    class GUIManager {

        public static ConsoleWindow consoleWindow;
        public static AccountStatusWindow accountStatusWindow;
        public static TrollboxWindow trollboxWindow;
        public static TickerFeedWindow tickerFeedWindow;
        public static ErrorWindow errorWindow;
        public static PerformanceWindow performanceWindow;
        public static TradeHistoryWindow tradeHistoryWindow;

        static string positionFilepath = "settings/windowPositions.file";
        private static Dictionary<string, FormCustom> windows;

        public static void Setup () {

            windows = new Dictionary<string, FormCustom>();

            consoleWindow = new ConsoleWindow();
            accountStatusWindow = new AccountStatusWindow();
            trollboxWindow = new TrollboxWindow();
            tickerFeedWindow = new TickerFeedWindow();
            errorWindow = new ErrorWindow();
            performanceWindow = new PerformanceWindow();
            tradeHistoryWindow = new TradeHistoryWindow();

            windows.Add(consoleWindow.ID, consoleWindow);
            windows.Add(accountStatusWindow.ID, accountStatusWindow);
            windows.Add(trollboxWindow.ID, trollboxWindow);
            windows.Add(tickerFeedWindow.ID, tickerFeedWindow);
            windows.Add(errorWindow.ID, errorWindow);
            windows.Add(performanceWindow.ID, performanceWindow);
            windows.Add(tradeHistoryWindow.ID, tradeHistoryWindow);

            // to ensure the handle is generated
            var temp = errorWindow.Handle;
            
            LoadPositions();

            ReloadAllGraphics();
        }
        public static void RestartThreads () {
            tickerFeedWindow.tickerFeed.Start();
            performanceWindow.performanceScreen.Start();
            tradeHistoryWindow.tradeHistoryScreen.Start();
        }

        public static void ReloadAllGraphics () {
            if (windows == null) return;

            FormCustom[] forms = windows.Values.ToArray();
            if (forms == null || forms.Length == 0) return;

            for (int i = 0; i < forms.Length; i++) {
                GenerateGraphicsWindow(forms[i]);
            }
        }
        private static void GenerateGraphicsWindow (System.Windows.Forms.Control control) {

            int width = control.Size.Width;
            int height = control.Size.Height;

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp)) {

                // first clear everything with full transparent
                using (Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0, 0))) {
                    g.FillRectangle(brush, 0, 0, width, height);
                }

                // draw white borders
                using (Brush brush = new SolidBrush(Color.FromArgb(192, 128, 128, 156))) {
                    using (Pen pen = new Pen(brush, 2)) {
                        g.DrawLine(pen, 2, 2, width - 2, 2);
                        g.DrawLine(pen, 2, 2, 2, height - 2);
                        g.DrawLine(pen, 2, height - 2, width - 2, height - 2);
                        g.DrawLine(pen, width - 2, 2, width - 2, height - 2);
                    }
                }

                // draw dark center
                using (Brush brush = new SolidBrush(Color.FromArgb(192, 0, 0, 0))) {
                    g.FillRectangle(brush, 4, 4, width - 8, height - 8);
                }
            }

            control.BackgroundImage = bmp;
        }
        private static void GenerateGraphicsButton (System.Windows.Forms.Control control) {

            int width = control.Size.Width;
            int height = control.Size.Height;

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp)) {

                // fill background
                using (Brush brush = new SolidBrush(Color.FromArgb(192, 0, 0, 0))) {
                    g.FillRectangle(brush, 0, 0, width, height);
                }

                // draw white borders
                using (Brush brush = new SolidBrush(Color.FromArgb(192, 128, 128, 156))) {
                    using (Pen pen = new Pen(brush, 1)) {
                        g.DrawLine(pen, 1, 1, width - 1, 1);
                        g.DrawLine(pen, 1, 1, 1, height - 1);
                        g.DrawLine(pen, 1, height - 1, width - 1, height - 1);
                        g.DrawLine(pen, width - 1, 1, width - 1, height - 1);
                    }
                }
            }

            control.BackgroundImage = bmp;
        }

        public static void SavePositions () {
            Console.WriteLine("Saving!");
            List<string> lines = new List<string>();

            FormCustom[] windowList = windows.Values.ToArray();
            for (int i = 0; i < windowList.Length; i++) {
                lines.Add("name=" + windowList[i].ID);
                lines.Add("visible=" + windowList[i].Visible);
                lines.Add("location=" + windowList[i].Location.X + "," + windowList[i].Location.Y);
                lines.Add("size=" + windowList[i].Size.Width + "," + windowList[i].Size.Height);
            }

            FileManager.SaveFile(positionFilepath, lines.ToArray());
        }
        static void LoadPositions () {

            string[] lines = FileManager.ReadFile(positionFilepath);
            FormCustom currEditedWindow = null;

            for (int i = 0; i < lines.Length; i++) {
                try {
                    string[] parts = lines[i].Split('=');
                    string propertyName = parts[0].ToLower().Trim();

                    switch (propertyName) {
                        case "name":
                            windows.TryGetValue(parts[1], out currEditedWindow);
                            break;
                        case "visible":
                            currEditedWindow.Visible = bool.Parse(parts[1]);
                            break;
                        case "location":
                            string[] locParts = parts[1].Split(',');
                            currEditedWindow.Location = new System.Drawing.Point(int.Parse(locParts[0]), int.Parse(locParts[1]));
                            break;
                        case "size":
                            string[] sizeParts = parts[1].Split(',');
                            currEditedWindow.Size = new System.Drawing.Size(int.Parse(sizeParts[0]), int.Parse(sizeParts[1]));
                            break;
                        default:
                            ErrorLog.ReportError("Unknown property name: \"" + parts[0] + "\" (line " + i + ")");
                            break;
                    }

                }
                catch (Exception e) {
                    ErrorLog.ReportError("Error while reading window positions (line " + i + ")", e);
                }
            }
        }
    }
}

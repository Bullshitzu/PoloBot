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
    public class StrategyControl : Templates.BaseControl {

        private const int screenCountX = 10;
        private const int screenCountY = 2;

        protected class PairData {

            public static double[] minimums = { 0, 0 };
            public static double[] maximums = { 5, 3 };
            public static string[] varTitles = { "M.Rev", "Vol." };
            public static string[] varNames = { "meanRev", "volumeTrend" };

            public string quoteName;
            public string baseName;
            public double[] variables;
            public bool blocked = false;

            public PairData (string quoteName, string baseName) {
                this.quoteName = quoteName;
                this.baseName = baseName;
                variables = null;
            }
            public PairData (string quoteName, string baseName, double[] vars) {
                this.quoteName = quoteName;
                this.baseName = baseName;
                variables = vars;
            }
        }

        private List<PairData> pairData;

        public void AddPairData (PoloniexAPI.CurrencyPair pair) {
            if (pairData == null) pairData = new List<PairData>();

            pairData.Add(new PairData(pair.QuoteCurrency, pair.BaseCurrency));

            UpdateModulesScreen();
        }
        public void ClearPairData () {
            if (pairData == null) return;

            pairData.Clear();

            UpdateModulesScreen();
        }
        public void UpdatePairData (PoloniexAPI.CurrencyPair pair, Dictionary<string, double> ruleVariables) {
            if (pairData == null) pairData = new List<PairData>();

            double[] vars = new double[PairData.varNames.Length];

            for (int i = 0; i < PairData.varNames.Length; i++) {
                double tempVar = 0;
                ruleVariables.TryGetValue(PairData.varNames[i], out tempVar);
                vars[i] = tempVar;
            }

            for (int i = 0; i < pairData.Count; i++) {
                if (pairData[i].quoteName == pair.QuoteCurrency) {
                    pairData[i] = new PairData(pair.QuoteCurrency, pair.BaseCurrency, vars);
                    return;
                }
            }
        }
        public void SetBlockedPairData (PoloniexAPI.CurrencyPair pair, bool state) {
            if (pairData == null) pairData = new List<PairData>();

            for (int i = 0; i < pairData.Count; i++) {
                if (pairData[i].quoteName == pair.QuoteCurrency) {
                    pairData[i].blocked = state;
                    return;
                }
            }
        }

        private void UpdateModulesScreen () {
            try {
                if (pairData == null) GUIManager.UpdateModulesPairs(null);
                else {
                    string[] lines = new string[pairData.Count];
                    for (int i = 0; i < pairData.Count; i++) {
                        lines[i] = pairData[i].quoteName;
                    }
                    GUIManager.UpdateModulesPairs(lines);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + "  - " + e.StackTrace);
            }
        }

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float boxWidth = (Width - 18) / (float)screenCountX;
            float boxHeight = (Height - 2) / (float)screenCountY;

            int pairDataIndex = 0;

            for (int y = 0; y < screenCountY; y++) {
                for (int x = 0; x < screenCountX; x++) {

                    float posX = x * (boxWidth + 2);
                    float posY = y * (boxHeight + 2);

                    RectangleF screenRect = new RectangleF(posX + 1, posY + 1, boxWidth - 2, boxHeight - 2);

                    try {
                        if (pairData == null || pairData.Count <= pairDataIndex) DrawNoDataSmall(g, screenRect);
                        else if (pairData[pairDataIndex].blocked) DrawPairBlocked(g, screenRect, pairData[pairDataIndex]);
                        else DrawPairBox(g, screenRect, pairData[pairDataIndex]);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message + "  - " + ex.StackTrace);
                    }

                    pairDataIndex++;
                    if (pairDataIndex >= 20) return;
                }
            }
        }

        private void DrawPairBox (Graphics g, RectangleF rect, PairData pairData) {

            // quote name on top

            string title = pairData.quoteName;
            float width = g.MeasureString(title, Style.Fonts.Small).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                g.DrawString(title, Style.Fonts.Small, brush, rect.X + (rect.Width / 2) - (width / 2), rect.Y + 5);
            }

            // base name in bottom right

            title = pairData.baseName;
            width = g.MeasureString(title, Style.Fonts.Small).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Dark1)) {
                g.DrawString(title, Style.Fonts.Small, brush, rect.X + rect.Width - width - 5, rect.Y + rect.Height - Style.Fonts.Small.Height - 5);
            }

            // variables
            if (pairData.variables != null) {
                float posY = rect.Y + 25;
                using (Brush brushBackground = new SolidBrush(Style.Colors.Primary.Dark2)) {
                    using (Brush brushValid = new SolidBrush(Style.Colors.Primary.Main)) {
                        using (Brush brushInvalid = new SolidBrush(Style.Colors.Primary.Dark1)) {
                            using (Brush brushText = new SolidBrush(Style.Colors.Background)) {
                                using (Pen pen = new Pen(brushText)) {
                                    for (int i = 0; i < pairData.variables.Length; i++) {

                                        // variable boxes

                                        g.FillRectangle(brushBackground, rect.X + 5, posY, rect.Width - 10, 15);

                                        // variable bar

                                        double min = PairData.minimums[i];
                                        double max = PairData.maximums[i];

                                        double mult = pairData.variables[i];
                                        if (mult < min) mult = 0;
                                        else if (mult > max) mult = 1;
                                        else mult = (mult - min) / (max - min);

                                        Brush barBrush = mult > 0.5 ? brushValid : brushInvalid;

                                        g.FillRectangle(barBrush, rect.X + 5, posY, (float)mult * (rect.Width - 10), 15);

                                        // variable name

                                        g.DrawString(PairData.varTitles[i], Style.Fonts.Small, brushText, new PointF(rect.X + 6, posY + 2));

                                        posY += 20;
                                    }

                                    // center line

                                    g.DrawLine(pen, rect.X + (rect.Width / 2), rect.Y + 20, rect.X + (rect.Width / 2), rect.Y + 80);
                                }
                            }
                        }
                    }
                }
            }


            using (Pen pen = new Pen(Style.Colors.Primary.Main, 2)) {
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        private void DrawPairBlocked (Graphics g, RectangleF rect, PairData pairData) {

            // quote name on top

            string title = pairData.quoteName;
            float width = g.MeasureString(title, Style.Fonts.Small).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Light1)) {
                g.DrawString(title, Style.Fonts.Small, brush, rect.X + (rect.Width / 2) - (width / 2), rect.Y + 5);
            }

            // blocked text

            string text = "BLOCKED";
            width = g.MeasureString(text, Style.Fonts.Small).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Secondary.Dark1)) {
                g.DrawString(text, Style.Fonts.Small, brush, rect.X + (rect.Width / 2) - (width / 2), rect.Y + (rect.Height / 2) - (Style.Fonts.Small.Height / 2));
            }

            // borders

            using (Pen pen = new Pen(Style.Colors.Primary.Main, 2)) {
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }

        }
    }
}

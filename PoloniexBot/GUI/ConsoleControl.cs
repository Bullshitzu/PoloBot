using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoloniexBot.CLI;

namespace PoloniexBot.GUI {
    public class ConsoleControl : Templates.BaseControl {

        public ConsoleControl () : base() {
            try {
                dragonGraphic = Properties.Resources.dragon;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + " - " + e.StackTrace);
            }
        }

        float topDivider = 28;

        private Control tbInput;

        private Image dragonGraphic=null;

        public void SetTBInput (Control control) {
            this.tbInput = control;
        }

        // -------------------------------------

        public void SetMessages (Manager.Message[] messages) {
            if (scrollPosition > 0) {
                if (this.messages != null && this.messages.Length < messages.Length)
                    scrollPosition += messages.Length - this.messages.Length;
            }

            this.messages = messages;
        }
        
        Manager.Message[] messages = null;

        public bool[] DrawMessageTypes = { true, true, true, false, true };

        public void ScrollMessages (int amount) {
            if (messages == null) return;

            scrollPosition += amount;

            if (scrollPosition < 0) scrollPosition = 0;
            if (scrollPosition >= messages.Length) scrollPosition = messages.Length - 1;
        }

        protected override void OnPaint (System.Windows.Forms.PaintEventArgs e) {

            Graphics g = e.Graphics;
            g.Clear(Style.Colors.Background);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dragon Graphic

            g.DrawImage(dragonGraphic, new RectangleF(Width * 0.2f, Height * 0.1f, Width * 0.6f, Height * 0.8f));

            // Output

            if (tbInput != null) {
                DrawOutputBox(g, new RectangleF(1, topDivider + 1, Width - 2, tbInput.Location.Y - this.Location.Y - topDivider - 6));
            }

            // Black Bars

            using (Brush brush = new SolidBrush(Style.Colors.Background)) {
                g.FillRectangle(brush, 0, 0, Width, topDivider);
                g.FillRectangle(brush, 0, tbInput.Location.Y - this.Location.Y - 5, Width, tbInput.Size.Height);
            }

            // Display bar

            string text = "Display:";
            float width = g.MeasureString(text, Style.Fonts.Small).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                g.DrawString(text, Style.Fonts.Small, brush, 6, 8);
            }

            // Dividers

            using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                g.DrawLine(pen, 1, topDivider, Width - 2, topDivider);
                g.DrawLine(pen, messageHeaderWidth, topDivider, messageHeaderWidth, tbInput.Location.Y - this.Location.Y - 5);
            }

            // Input

            if (tbInput != null) {
                float posY = tbInput.Location.Y - this.Location.Y - 4;
                using (Pen pen = new Pen(Style.Colors.Primary.Dark2, 2)) {
                    g.DrawLine(pen, 1, posY, Width - 2, posY);
                }
            }

            // Draw borders
            DrawBorders(g);

        }

        // ---------------------------

        int scrollPosition = 0;
        int rowHeight = 20;
        int messageHeaderWidth = 70;
        int messageRowStart = 30;

        // ---------------------------

        private void DrawOutputBox (Graphics g, RectangleF rect) {
            if (messages == null) return;

            // --------------------

            if (scrollPosition > 0) {
                DrawScrollIcon(g, rect);
            }

            for (int i = scrollPosition; i < messages.Length; i++) {
                if (!CheckShouldDraw(messages[i])) continue;

                int rows = DrawMessage(g, messages[i], rect);
                float shrinkAmount = rows * rowHeight;

                if (rect.Height - shrinkAmount < 0) return;
                rect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height - shrinkAmount);
            }
        }

        private void DrawScrollIcon (Graphics g, RectangleF rect) {

            RectangleF smallRect = new RectangleF(rect.X + (rect.Width * 0.7f), rect.Y + (rect.Height * 0.5f), Style.Fonts.Reduced.Height * 4, Style.Fonts.Reduced.Height * 2);
            smallRect = new RectangleF(rect.X + rect.Width - smallRect.Width - 5, rect.Y + rect.Height - smallRect.Height - 5, smallRect.Width, smallRect.Height);


            using (Brush brush = new SolidBrush(Style.Colors.Background)) {
                g.FillRectangle(brush, smallRect);
            }

            using (Pen pen = new Pen(Style.Colors.Primary.Main, 2)) {
                g.DrawRectangle(pen, smallRect.X, smallRect.Y, smallRect.Width, smallRect.Height);

                float halfWidth = smallRect.Width / 2;
                float halfHeight = smallRect.Height / 2;

                g.DrawLine(pen, smallRect.X + halfWidth - halfHeight, smallRect.Y + halfHeight, smallRect.X + halfWidth, smallRect.Y + smallRect.Height - 5);
                g.DrawLine(pen, smallRect.X + +halfWidth + halfHeight, smallRect.Y + halfHeight, smallRect.X + halfWidth, smallRect.Y + smallRect.Height - 5);

            }

            string text = "+" + scrollPosition;
            float width = g.MeasureString(text, Style.Fonts.Reduced).Width;

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Main)) {
                g.DrawString(text, Style.Fonts.Reduced, brush, smallRect.X + (smallRect.Width / 2) - (width / 2), smallRect.Y + 2);
            }
        }

        private int DrawMessage (Graphics g, Manager.Message m, RectangleF rect) {

            Font messageFont = Style.Fonts.Small;

            string[] rows = DivideMessageIntoRows(g, m, messageFont, rect);
            if (rows == null) return 0;

            // draw time header

            float posY = rect.Y + rect.Height - (rows.Length * rowHeight);

            if (m.type != Manager.MessageType.NoHeader) {

                using (Pen pen = new Pen(Style.Colors.Primary.Dark2)) {
                    g.DrawLine(pen, rect.X, posY - 4, messageHeaderWidth, posY - 4);
                }

                string text = m.date.ToString("HH:mm:ss");
                using (Brush brush = new SolidBrush(Style.Colors.Primary.Dark1)) {
                    g.DrawString(text, Style.Fonts.Small, brush, rect.X + 5, posY);
                }
            }

            // draw type header

            float typeWidth = GetMessageTypeWidth(g, m, true, posY);

            // draw rows

            DrawMessageRows(g, rows, messageFont, messageHeaderWidth + typeWidth + 5, posY);

            return rows.Length;
        }

        private void DrawMessageRows (Graphics g, string[] rows, Font font, float posX, float posY) {

            using (Brush brush = new SolidBrush(Style.Colors.Primary.Dark1)) {
                for (int i = 0; i < rows.Length; i++) {

                    g.DrawString(rows[i], font, brush, posX, posY);

                    posY += rowHeight;
                    posX = messageHeaderWidth + messageRowStart;

                }
            }
        }

        private string[] DivideMessageIntoRows (Graphics g, Manager.Message m, Font font, RectangleF rect) {

            float spaceSize = g.MeasureString(" ", font).Width;
            string[] words = m.message.Split(' ');

            List<string> lines = new List<string>();
            string currLine = "";

            float posX = messageHeaderWidth + GetMessageTypeWidth(g, m, false, 0);

            for (int i = 0; i < words.Length; i++) {

                float wordWidth = g.MeasureString(words[i], font).Width;
                if (posX + wordWidth > rect.Width) {
                    lines.Add(currLine);
                    currLine = "";
                    posX = messageRowStart;
                }

                currLine += words[i] + " ";
                posX += wordWidth + spaceSize;
            }

            lines.Add(currLine);

            return lines.ToArray();
        }

        private float GetMessageTypeWidth (Graphics g, Manager.Message m, bool draw, float posY) {
            if (m.type == Manager.MessageType.NoHeader) return 0;
            string text = m.type.ToString() + ": ";
            float width = g.MeasureString(text, Style.Fonts.Small).Width;

            if (!draw) return width;

            Color color = GetTypeColor(m.type);
            using (Brush brush = new SolidBrush(color)) {
                g.DrawString(m.type.ToString() + ":", Style.Fonts.Small, brush, messageHeaderWidth + 5, posY);
            }

            return width;
        }

        private Color GetTypeColor (Manager.MessageType mType) {
            switch (mType) {
                case Manager.MessageType.User:
                    return Style.Colors.Primary.Light1;
                case Manager.MessageType.Note:
                    return Style.Colors.Primary.Main;
                case Manager.MessageType.Log:
                    return Style.Colors.Primary.Main;
                case Manager.MessageType.Warning:
                    return Style.Colors.Terciary.Main;
                case Manager.MessageType.Error:
                    return Style.Colors.Secondary.Main;
                case Manager.MessageType.NoHeader:
                    return Style.Colors.Primary.Main;
                default:
                    return Style.Colors.Primary.Main;
            }
        }

        private bool CheckShouldDraw (Manager.Message m) {
            switch (m.type) {
                case CLI.Manager.MessageType.NoHeader:
                    return true;
                case CLI.Manager.MessageType.User:
                    return DrawMessageTypes[0];
                case CLI.Manager.MessageType.Log:
                    return DrawMessageTypes[1];
                case CLI.Manager.MessageType.Note:
                    return DrawMessageTypes[2];
                case CLI.Manager.MessageType.Warning:
                    return DrawMessageTypes[3];
                case CLI.Manager.MessageType.Error:
                    return DrawMessageTypes[4];
                default:
                    return false;
            }


        }
    }
}

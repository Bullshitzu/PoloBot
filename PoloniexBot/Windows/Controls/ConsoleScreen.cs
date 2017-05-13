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
    public partial class ConsoleScreen : Label {
        public ConsoleScreen () {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Messages = new CLI.Manager.Message[0];
        }

        Brush brushUser = new SolidBrush(Color.Silver);
        Brush brushLight = new SolidBrush(Color.Gray);
        Brush brushNote = new SolidBrush(Color.DimGray);
        Brush brushLog = new SolidBrush(Color.SteelBlue);
        Brush brushWarning = new SolidBrush(Color.Olive);
        Brush brushError = new SolidBrush(Color.DarkRed);

        Brush brushTimestamp = new SolidBrush(Color.DimGray);
        Brush messageBrush = new SolidBrush(Color.DimGray);

        public CLI.Manager.Message[] Messages;

        protected override void OnPaint (PaintEventArgs e) {

            Graphics g = e.Graphics;
            Font font = this.Font;

            g.Clear(this.BackColor);

            float posY = this.Height - font.Height - 5;

            float lineHeight = font.Height + 3;

            for (int i = 0; i < Messages.Length; i++) {
                float posX = 5;

                Brush typeBrush;
                Brush msgBrush = messageBrush;

                bool printHeader = true;

                float posMove = 2;
                switch (Messages[i].type) {
                    case CLI.Manager.MessageType.User:
                        typeBrush = brushUser;
                        posMove = 126;
                        break;
                    case CLI.Manager.MessageType.Log:
                        typeBrush = brushLog;
                        posMove = 121;
                        break;
                    case CLI.Manager.MessageType.Warning:
                        typeBrush = brushWarning;
                        posMove = 148;
                        break;
                    case CLI.Manager.MessageType.Error:
                        typeBrush = brushError;
                        posMove = 128;
                        break;
                    case CLI.Manager.MessageType.NoHeader:
                        typeBrush = brushNote;
                        msgBrush = brushLight;
                        posMove = 0;
                        printHeader = false;
                        break;
                    default:
                        typeBrush = brushNote;
                        posMove = 126;
                        break;
                }

                string[] words = Messages[i].message.Split(' ');

                float simX = posMove;
                int rowCount=0;

                for (int j = 0; j < words.Length; j++) {
                    float width = g.MeasureString(words[j], font).Width;
                    if (simX + width > this.Width) {
                        rowCount++;
                        simX = 40;
                    }
                    else simX += width - 2;
                }

                // ---------

                if (printHeader) {
                    g.DrawString(Messages[i].date.ToString("HH:mm:ss") + ":", font, brushTimestamp, 5, posY - (lineHeight * rowCount));
                    g.DrawString("[" + Messages[i].type.ToString() + "] - ", font, typeBrush, 70, posY - (lineHeight * rowCount));
                    posX = posMove;
                }
                else posX = 5 + posMove;

                // ----------

                float simY = posY - (lineHeight * rowCount);

                for (int j = 0; j < words.Length; j++) {
                    float width = g.MeasureString(words[j], font).Width;
                    if (posX + width > this.Width) {
                        simY += lineHeight;
                        posX = 20 + width;
                        g.DrawString(words[j], font, msgBrush, 20, simY);
                    }
                    else {
                        g.DrawString(words[j], font, msgBrush, posX, simY);
                        posX += width - 2;
                    }
                }

                posY -= lineHeight * (rowCount+1);
            }
        }
    }
}

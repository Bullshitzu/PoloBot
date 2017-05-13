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
    public partial class Chatbox : Label {
        public Chatbox () {
            InitializeComponent();
            Messages = new List<PoloniexAPI.TrollboxMessageEventArgs>();
        }

        public List<PoloniexAPI.TrollboxMessageEventArgs> Messages { get; set; }

        protected override void OnPaint (PaintEventArgs e) {

            if (Messages == null) Messages = new List<PoloniexAPI.TrollboxMessageEventArgs>();

            while (Messages.Count > 30) Messages.RemoveAt(Messages.Count - 1);

            Graphics g = e.Graphics;
            Font font = this.Font;
            Brush brush1 = new SolidBrush(Color.SteelBlue);
            Brush brushMe = new SolidBrush(Color.Red);
            Brush brush2;

            g.Clear(this.BackColor);

            float lineSpacing = 3.0f;

            float posX = 10.0f;
            float posY = this.Height - 3;

            int posYMoves = 0;

            for (int i = 0; i < Messages.Count; i++) {
                if (posY < 0) return;


                if (Messages[i].SenderReputation > 10000) brush2 = new SolidBrush(Color.WhiteSmoke);
                else if (Messages[i].SenderReputation > 7000) brush2 = new SolidBrush(Color.Gainsboro);
                else if (Messages[i].SenderReputation > 3000) brush2 = new SolidBrush(Color.LightGray);
                else if (Messages[i].SenderReputation > 1000) brush2 = new SolidBrush(Color.Silver);
                else if (Messages[i].SenderReputation > 500) brush2 = new SolidBrush(Color.DarkGray);
                else if (Messages[i].SenderReputation > 250) brush2 = new SolidBrush(Color.Gray);
                else brush2 = new SolidBrush(Color.DimGray);

                posY -= font.Height * posYMoves;
                posX = 3.0f;

                string line = Messages[i].SenderName+": "+Messages[i].MessageText;
                string[] words = line.Split(' ');

                SizeF size = g.MeasureString(line, font);
                
                int lineCount = 1;
                for (int j = 0; j < words.Length; j++) {
                    SizeF charSize = g.MeasureString(words[j], font);
                    if (posX + charSize.Width > this.Width) {
                        lineCount++;
                        posX = 10.0f;
                    }
                    posX += charSize.Width - 2;
                }

                posY -= lineCount * (size.Height + lineSpacing);
                posX = 3.0f;

                for (int j = 0; j < words.Length; j++) {
                    string charToDraw = words[j];
                    SizeF charSize = g.MeasureString(charToDraw, font);

                    if (posX + charSize.Width > this.Width) {
                        posY += charSize.Height + lineSpacing;
                        posX = 10.0f;
                    }

                    Brush brush = j == 0 ? brush1 : brush2;

                    if (charToDraw == "KinkyHyo,") brush = brushMe;

                    g.DrawString(charToDraw, font, brush, posX, posY);
                    posX += charSize.Width - 2;
                }

                posY -= (size.Height + lineSpacing) * (lineCount-1);
            }
        }
    }
}

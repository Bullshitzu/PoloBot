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
using PoloniexBot.Data;

namespace PoloniexBot.Windows.Controls {
    public partial class ScoreScreen : MultiThreadControl {
        public ScoreScreen () {
            InitializeComponent();
        }

        ResultSet[] data;
        double score;
        double volatility;

        public void UpdateData (params ResultSet[] results) {
            if (results != null) data = results;
        }

        public void UpdateScore (double value) {
            this.score = value;
        }
        public void UpdateVolatility (double value) {
            this.volatility = value;
        }

        private Brush brushText = new SolidBrush(Color.FromArgb(107, 144, 148));
        private Font font = new System.Drawing.Font("Calibri Bold Caps", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        private float stepY = 12;

        protected override void Draw (Graphics g) {
            threadName = "(GUI) Score";

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.Clear(this.BackColor);

            if (data == null) return;

            font = this.Font;

            float posY = -3;

            using (Pen penText = new Pen(brushText)) {
                for (int i = 0; i < data.Length; i++) {

                    float textWidth = g.MeasureString(data[i].signature, font).Width;
                    float xPos = (Width / 2) - (textWidth / 2);

                    posY += 5;
                    g.DrawLine(penText, 5, posY + stepY * 0.75f, xPos - 10, posY + stepY * 0.75f);
                    g.DrawString(data[i].signature, font, brushText, xPos, posY);
                    g.DrawLine(penText, xPos + textWidth + 5, posY + stepY * 0.75f, Width - 10, posY + stepY * 0.75f);
                    posY += stepY + 5;

                    ResultSet.Variable[] vars = data[i].variables.Values.ToArray();

                    for (int j = 0; j < vars.Length; j++) {
                        g.DrawString(vars[j].name, font, brushText, 5, posY);
                        g.DrawString(vars[j].value.ToString("F" + vars[j].roundCount), font, brushText, this.Width / 2, posY);
                        posY += stepY;
                    }
                }

                float tw = g.MeasureString("Total", font).Width;
                float xP = (Width / 2) - (tw / 2);

                posY += 5;
                g.DrawLine(penText, 5, posY + stepY * 0.75f, xP - 10, posY + stepY * 0.75f);
                g.DrawString("Total", font, brushText, xP, posY);
                g.DrawLine(penText, xP + tw + 5, posY + stepY * 0.75f, Width - 10, posY + stepY * 0.75f);
                posY += stepY + 5;

                g.DrawString("Score", font, brushText, 5, posY);
                g.DrawString(score.ToString("F4"), font, brushText, this.Width / 2, posY);
                posY += stepY;

                g.DrawString("Volatility", font, brushText, 5, posY);
                g.DrawString(volatility.ToString("F4"), font, brushText, this.Width / 2, posY);
                posY += stepY;
            }
        }
    }
}

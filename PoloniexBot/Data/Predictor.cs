using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;

namespace PoloniexBot.Data {
    abstract class Predictor : IDisposable {

        protected CurrencyPair pair;
        protected List<ResultSet> results;
        protected bool drawEnabled = true;
        public const long drawTimeframe = 1800;

        public Predictor (CurrencyPair pair) {
            this.pair = pair;
            this.results = new List<ResultSet>();
        }

        public virtual void Dispose () { }

        public ResultSet GetLastResult () {
            if (results == null || results.Count == 0) return null;
            return results.Last();
        }
        public ResultSet[] GetAllResults () {
            return results.ToArray();
        }

        public void SaveResult (ResultSet rs) {
            SignResult(rs);
            results.Add(rs);
        }

        public abstract void SignResult (ResultSet rs);

        public virtual void DrawPredictor (Graphics g, long timePeriod, RectangleF rect) {

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            string line = "Predictor Display Not Implemented";
            using (Font font = new System.Drawing.Font("Impact", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)))) {

                SizeF size = g.MeasureString(line, font);

                PointF point = new PointF(rect.Width / 2 - (size.Width / 2), rect.Height / 2 - (size.Height / 2));
                point = new PointF(point.X + rect.X, point.Y + rect.Y);

                using (Brush brush = new SolidBrush(Color.Red)) {
                    g.DrawString(line, font, brush, point);
                }
            }

            g.SmoothingMode = oldSmoothingMode;
        }

        protected void DrawNoData (Graphics g, RectangleF rect) {

            System.Drawing.Drawing2D.SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            string line = "No Data";
            using (Font font = new System.Drawing.Font("Impact", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)))) {

                SizeF size = g.MeasureString(line, font);

                PointF point = new PointF(rect.Width / 2 - (size.Width / 2), rect.Height / 2 - (size.Height / 2));
                point = new PointF(point.X + rect.X, point.Y + rect.Y);

                using (Brush brush = new SolidBrush(Color.Red)) {
                    g.DrawString(line, font, brush, point);
                }
            }

            g.SmoothingMode = oldSmoothingMode;

        }
    }
}

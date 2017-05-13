using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace PoloniexBot.Windows.Controls {
    public partial class MultiThreadControl : UserControl {
        public MultiThreadControl () {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        public const int Framerate = 30;
        private static int frameCounter = 0;

        public void Start () {
            if (thread != null) Utility.ThreadManager.Kill(thread);
            thread = Utility.ThreadManager.Register(DrawLoop, threadName+frameCounter, false, 100);
            frameCounter++;
        }
        public void Stop () {
            if (thread != null) Utility.ThreadManager.Kill(thread);
        }

        private Thread thread;
        private Bitmap buffer;

        protected string threadName = "GUI";

        private void DrawLoop () {
            int delay = 1000 / Framerate;
            while (true) {
                lock (this) {
                    if (buffer != null) buffer.Dispose();
                    buffer = new Bitmap(Size.Width, Size.Height);
                    using (Graphics g = Graphics.FromImage(buffer)) {
                        Draw(g);
                    }
                }
                this.Invoke(new MethodInvoker(Invalidate));

                Utility.ThreadManager.ReportAlive(threadName);
                System.Threading.Thread.Sleep(delay);
            }
        }
        protected virtual void Draw (Graphics g) { }

        protected override void OnPaint (PaintEventArgs e) {
            lock (this) {
                e.Graphics.DrawImageUnscaled(buffer, 0, 0);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace PoloniexBot.GUI.Style {
    public class Colors {

        public struct Palette {

            public Color Main;
            public Color Light1;
            public Color Light2;
            public Color Dark1;
            public Color Dark2;

            public Palette (Color main, Color light1, Color light2, Color dark1, Color dark2) {
                Main = main;
                Light1 = light1;
                Light2 = light2;
                Dark1 = dark1;
                Dark2 = dark2;
            }
        }

        public static Color Background { get { return _Background; } }
        public static Color Positive { get { return _Positive; } }
        public static Color Negative { get { return _Negative; } }

        public static Palette Primary { get { return blues1; } }
        public static Palette Secondary { get { return reds1; } }
        public static Palette Terciary { get { return golds1; } }

        // ----------------
        // Colors
        // ----------------

        private static Color _Background = Color.FromArgb(0, 0, 0);
        private static Color _Positive = Color.FromArgb(37, 216, 37);
        private static Color _Negative = Color.FromArgb(216, 37, 37);

        private static Palette blues1 = new Palette(
            Color.FromArgb(16, 102, 167),
            Color.FromArgb(55, 133, 192), Color.FromArgb(101, 174, 230),
            Color.FromArgb(7, 70, 125), Color.FromArgb(4, 48, 83));

        private static Palette golds1 = new Palette(
            Color.FromArgb(216, 185, 37),
            Color.FromArgb(232, 206, 77), Color.FromArgb(255, 232, 120),
            Color.FromArgb(172, 146, 19), Color.FromArgb(133, 110, 0));

        private static Palette reds1 = new Palette(
            Color.FromArgb(216, 37, 37),
            Color.FromArgb(232, 77, 77), Color.FromArgb(255, 120, 120),
            Color.FromArgb(172, 19, 19), Color.FromArgb(133, 0, 0));

    }
}

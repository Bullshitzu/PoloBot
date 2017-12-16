using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace PoloniexBot.GUI.Style {
    public class Fonts {

        private static Font _Title = new System.Drawing.Font("Unispace", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        public static Font Title { get { return _Title; } }


        private static Font _Medium = new System.Drawing.Font("Unispace", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        public static Font Medium { get { return _Medium; } }

        private static Font _Reduced = new System.Drawing.Font("Unispace", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        public static Font Reduced { get { return _Reduced; } }

        private static Font _Small = new System.Drawing.Font("Unispace", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        public static Font Small { get { return _Small; } }

        private static Font _Tiny = new System.Drawing.Font("Unispace", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        public static Font Tiny { get { return _Tiny; } }

    }
}

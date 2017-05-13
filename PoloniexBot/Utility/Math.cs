using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility {
    public static class Math {
        
        // Lerp

        public static long Lerp (long a, long b, float t) {
            return (long)Lerp((double)a, (double)b, t);
        }
        public static int Lerp (int a, int b, float t) {
            return (int)Lerp((double)a, (double)b, t);
        }
        public static float Lerp (float a, float b, float t) {
            return (float)Lerp((double)a, (double)b, t);
        }
        public static double Lerp (double a, double b, float t) {
            return a + ((b - a) * t);
        }
    }
}

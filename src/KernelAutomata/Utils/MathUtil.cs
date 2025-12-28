using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Utils
{
    public static class MathUtil
    {
        public static void MeanStd(float[] data, out float mean, out float std)
        {
            int n = data.Length;
            if (n == 0)
            {
                mean = std = 0f;
                return;
            }

            double sum = 0.0;
            for (int i = 0; i < n; i++)
                sum += data[i];

            mean = (float)(sum / n);

            double var = 0.0;
            for (int i = 0; i < n; i++)
            {
                double d = data[i] - mean;
                var += d * d;
            }

            std = (float)Math.Sqrt(var / n);
        }

        public static double GetTorusDistance(double d1, double d2, double size)
        {
            double d = d2 - d1;
            if (Math.Abs(d) > size / 2)
            {
                d = d - size * Math.Sign(d);
            }

            return d;
        }

        public static double Growth(double u, double mu, double sigma)
        {
            double x = (u - mu) / sigma;
            return (2.0 * Math.Exp(-x * x) - 1.0);
        }

        public static float GaussianBell(float r, float center, float width, float innerSlope = 1.0f, float outerSlope = 1.0f)
        {
            var val = (float)Math.Exp(-(r - center) * (r - center) / (2 * width * width));
            if (r > center && outerSlope != 0 && outerSlope != 1)
                val = (float)Math.Pow(val, outerSlope);
            if (r < center && innerSlope != 0 && innerSlope != 1)
                val = (float)Math.Pow(val, innerSlope);
            return val;
        }
    }
}

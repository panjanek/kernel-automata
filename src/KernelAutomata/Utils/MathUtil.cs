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
    }
}

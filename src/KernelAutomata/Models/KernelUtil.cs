using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public static class KernelUtil
    {
        public static float[] Flatten4Channels(float[,] array2D, int channel)
        {
            int idx = 0;
            int size = array2D.GetLength(0);
            float[] result = new float[size * size * 4];
            for(int x=0;x< size; x++)
            {
                for(int y=0; y< size; y++)
                {
                    result[idx*4+channel] = array2D[x,y];
                    idx++;
                }
            }

            return result;
        }

        public static float[,] CreateRingKernel(
                                    int N,
                                    float R,
                                    float ringCenter,
                                    float ringWidth)
        {
            float[,] kernel = new float[N, N];
            float sum = 0f;

            for (int y = 0; y < N; y++)
            {
                int dy = (y <= N / 2) ? y : y - N;

                for (int x = 0; x < N; x++)
                {
                    int dx = (x <= N / 2) ? x : x - N;

                    float r = MathF.Sqrt(dx * dx + dy * dy);

                    if (r > R)
                    {
                        kernel[x, y] = 0f;
                        continue;
                    }

                    // Normalize radius to [0,1]
                    float rn = r / R;

                    // Distance from ring center
                    float t = (rn - ringCenter) / ringWidth;

                    float v = SmoothBump(t);

                    kernel[x, y] = v;
                    sum += v;
                }
            }

            // Normalize so sum(kernel) = 1
            if (sum > 0f)
            {
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                        kernel[x, y] /= sum;
            }

            return kernel;
        }


        private static float SmoothBump(float x)
        {
            // Compact support in (-1, 1)
            if (Math.Abs(x) >= 1f)
                return 0f;

            // Standard C-infinity bump
            return MathF.Exp(-1f / (1f - x * x));
        }
    }
}

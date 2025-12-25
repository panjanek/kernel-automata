using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Utils
{
    public static class KernelUtil
    {
        public static float[] SumKernels(float[] kernel1, float weight1)
        {
            float[] result = new float[kernel1.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = kernel1[i] * weight1;
            }

            return result;
        }
        public static float[] SumKernels(float[] kernel1, float weight1, float[] kernel2, float weight2)
        {
            float[] result = new float[kernel1.Length];
            for(int i=0; i<result.Length; i++)
            {
                result[i] = kernel1[i] * weight1 + kernel2[i] * weight2;
            }

            return result;
        }

        public static float[] SumKernels(float[] kernel1, float weight1, float[] kernel2, float weight2, float[] kernel3, float weight3)
        {
            float[] result = new float[kernel1.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = kernel1[i] * weight1 + kernel2[i] * weight2 + kernel3[i] * weight3;
            }

            return result;
        }

        public static float[] CreateGausianRing(int N, float R, float ringCenter, float ringWidth)
        {
            float[,] kernel = new float[N, N];
            float sum = 0f;

            for (int y = 0; y < N; y++)
            {
                int dy = y <= N / 2 ? y : y - N;

                for (int x = 0; x < N; x++)
                {
                    int dx = x <= N / 2 ? x : x - N;

                    float r = MathF.Sqrt(dx * dx + dy * dy);

                    if (r > R)
                    {
                        kernel[x, y] = 0f;
                        continue;
                    }

                    // Normalize radius to [0,1]
                    //float rn = r / R;

                    // Distance from ring center
                    //float t = (r - ringCenter) / ringWidth;

                    float v = Gauss(r, ringCenter, ringWidth);

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

            return Flatten4Channels(kernel, 0);
        }

        public static float Gauss(float r, float r1, float sigma)
        {
            return (float)Math.Exp( -(r-r1)*(r-r1) / (2*sigma*sigma) );
        }

        private static float[] Flatten4Channels(float[,] array2D, int channel)
        {
            int idx = 0;
            int size = array2D.GetLength(0);
            float[] result = new float[size * size * 4];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    result[idx * 4 + channel] = array2D[x, y];
                    idx++;
                }
            }

            return result;
        }
    }
}

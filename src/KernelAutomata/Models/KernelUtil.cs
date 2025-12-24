using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static float[,] CreateGausianRing(int N, float R, float ringCenter, float ringWidth)
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

            return kernel;
        }

        public static float Gauss(float r, float r1, float sigma)
        {
            return (float)Math.Exp( -(r-r1)*(r-r1) / (2*sigma*sigma) );
        }

        public static float[,] CreateRingKernel(int N, float R,float ringCenter,float ringWidth)
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




        public static float[,] CreateRingKernel2(
            int N,
            float R,
            float ringCenter,
            float ringWidth)
        {
            float[,] kernel = new float[N, N];
            float sum = 0f;

            // Negative lobe parameters (critical for movers)
            float alpha = 0.7f;   // strength of negative lobe
            float beta = 2.0f;   // width multiplier for negative lobe

            for (int y = 0; y < N; y++)
            {
                // Wrap coordinates so (0,0) is kernel center
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

                    float v = 0f;

                    // ---------- positive ring ----------
                    float t1 = (rn - ringCenter) / ringWidth;
                    if (MathF.Abs(t1) <= 3f)
                    {
                        v += MathF.Exp(-0.5f * t1 * t1);
                    }

                    // ---------- negative ring (broader) ----------
                    float t2 = (rn - ringCenter) / (beta * ringWidth);
                    if (MathF.Abs(t2) <= 3f)
                    {
                        v -= alpha * MathF.Exp(-0.5f * t2 * t2);
                    }

                    kernel[x, y] = v;
                    sum += v;
                }
            }

            // Normalize so sum(kernel) = 1
            if (MathF.Abs(sum) > 1e-8f)
            {
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                        kernel[x, y] /= sum;
            }

            return kernel;
        }

        private static float Bump(float x, float max, float width)
        {
            float t = ((x - max) / width);
            return (float)(2 * Math.Exp(-t * t));
        }

        public static float[,] CreateRingKernel3(
            int N,
            float R,
            float ringCenter,
            float ringWidth)
        {
            float[,] kernel = new float[N, N];
            float sum = 0f;

            // Negative lobe parameters (critical for movers)
            float alpha = 0.7f;   // strength of negative lobe
            float beta = 2.0f;   // width multiplier for negative lobe

            for (int y = 0; y < N; y++)
            {
                // Wrap coordinates so (0,0) is kernel center
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

                    float v = 0f;

                    ringWidth = 0.05f;

                    v += Bump(rn, 0.5f, 0.1f);

                    

                    v += 0.7f*Bump(rn, 0.8f, 0.075f);


                    v = v / 2;

                    //v -= 1*Bump(rn, 0.7f, 0.5f);

                    /*
                    // inner competing ring
                    float t1b = (rn - ringCenter * 0.85f) / ringWidth;
                    if (MathF.Abs(t1b) <= 3f)
                        v -= 0.4f * MathF.Exp(-0.5f * t1b * t1b);

                    // outer negative shoulder (as before)
                    float t2 = (rn - ringCenter * 0.85f) / ringWidth; //(rn - ringCenter) / (2.0f * ringWidth);
                    if (MathF.Abs(t2) <= 3f)
                        v -= 0.6f * MathF.Exp(-0.5f * t2 * t2);
                    */

                    if (v < 0)
                        v = 0;

                    if (v > 1)
                        v = 1;

                    kernel[x, y] = v;
                    sum += v;
                }
            }

            // Normalize so sum(kernel) = 1
            if (MathF.Abs(sum) > 1e-8f)
            {
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                        kernel[x, y] /= sum;
            }

            return kernel;
        }





    }
}

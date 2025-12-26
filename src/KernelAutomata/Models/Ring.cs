using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public class Ring
    {
        public float maxR;

        public float center;

        public float width;

        public float weight;

        public float[] ringBuffer;

        private int fieldSize;
        public Ring(int size)
        {
            fieldSize = size;
            ringBuffer = new float[fieldSize * fieldSize * 4];
        }

        public void Set(float maxR, float center, float width, float weight)
        {
            this.maxR = maxR;
            this.center = center;
            this.width = width;
            this.weight = weight;
            Recalculate();
        }

        public void Recalculate()
        {
            if (weight == 0)
            {
                Array.Fill<float>(ringBuffer, 0);
                return;
            }

            int N = fieldSize;

            //float[,] kernel = new float[N, N];
            float sum = 0f;

            for (int y = 0; y < N; y++)
            {
                int dy = y <= N / 2 ? y : y - N;

                for (int x = 0; x < N; x++)
                {
                    int dx = x <= N / 2 ? x : x - N;

                    float r = MathF.Sqrt(dx * dx + dy * dy);

                    if (r > maxR)
                    {
                        //kernel[x, y] = 0f;
                        ringBuffer[(x * fieldSize + y)*4] = 0f;
                        continue;
                    }

                    float v = Gauss(r, center, width);
                    //kernel[x, y] = v;
                    ringBuffer[(x * fieldSize + y) * 4] = v;
                    sum += v;
                }
            }

            // Normalize so sum(kernel) = 1
            if (sum > 0f)
            {
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                        //kernel[x, y] /= sum;
                        ringBuffer[(x * fieldSize + y) * 4] /= sum;
            }

            //Flatten4Channels(kernel, 0, ringBuffer);
        }

        private static void Flatten4Channels(float[,] array2D, int channel, float[] output)
        {
            int idx = 0;
            int size = array2D.GetLength(0);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    output[idx * 4 + channel] = array2D[x, y];
                    idx++;
                }
            }
        }

        public static float Gauss(float r, float r1, float sigma)
        {
            return (float)Math.Exp(-(r - r1) * (r - r1) / (2 * sigma * sigma));
        }
    }
}

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

        public float innerSlope;

        public float outerSlope;

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
                        ringBuffer[(x * fieldSize + y)*4] = 0f;
                        continue;
                    }

                    float v = GaussianBell(r, center, width, innerSlope, outerSlope);
                    ringBuffer[(x * fieldSize + y) * 4] = v;
                    sum += v;
                }
            }

            // Normalize so sum(kernel) = 1
            if (sum > 0f)
            {
                for (int i = 0; i < ringBuffer.Length; i++)
                    ringBuffer[i] /= sum;
            }
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

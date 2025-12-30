using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Utils;

namespace KernelAutomata.Models
{
    public class RingRecipe
    {
        public float maxR;

        public float center;

        public float width;

        public float weight;

        public float innerSlope = 1.0f;

        public float outerSlope = 1.0f;

        public RingRecipe Clone()
        {
            return new RingRecipe()
            {
                maxR = maxR,
                center = center,
                innerSlope = innerSlope,
                outerSlope = outerSlope,
                weight = weight,
                width = width,
            };
        }

        public void OverwriteWith(RingRecipe recipe)
        {
            maxR = recipe.maxR;
            center = recipe.center;
            width = recipe.width;
            weight = recipe.weight;
            innerSlope = recipe.innerSlope;
            outerSlope = recipe.outerSlope;
        }

        public void Invert()
        {
            weight = -weight;
        }

        public void ChangeCenter(float delta)
        {
            center += delta;
            if (center < 0)
                center = 0;
            if (delta > 0)
                maxR += delta;
        }

        public void FillBuffer(float[] ringBuffer, int fieldSize)
        {
            Array.Fill<float>(ringBuffer, 0);
            if (weight == 0)
                return;

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
                        ringBuffer[(x * fieldSize + y) * 4] = 0f;
                        continue;
                    }

                    float v = MathUtil.GaussianBell(r, center, width, innerSlope, outerSlope);
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
    }
}

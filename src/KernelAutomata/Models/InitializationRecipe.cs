using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Utils;

namespace KernelAutomata.Models
{
    public class InitializationRecipe
    {
        public float centerX;

        public float centerY;

        public float density;

        public float noiseRadius;

        public float blobRadius;

        public float mu;

        public float sigma;

        public InitializationRecipe Clone()
        {
            return new InitializationRecipe()
            {
                centerX = centerX,
                blobRadius = blobRadius,
                mu = mu,
                sigma = sigma,
                centerY = centerY,
                density = density,
                noiseRadius = noiseRadius
            };
        }

        public void OverwriteWith(InitializationRecipe recipe)
        {
            centerX = recipe.centerX;
            centerY = recipe.centerY;
            density = recipe.density;
            noiseRadius = recipe.noiseRadius;
            blobRadius = recipe.blobRadius;
            mu = recipe.mu;
            sigma = recipe.sigma;
        }

        public void FillInitBufferWithRandomData(int size, float[] initBuffer)
        {
            Random rng = new Random(1);

            for (int i = 0; i < size * size; i++)
            {
                var rnd = (float)rng.NextDouble();
                initBuffer[4 * i + 0] = rnd * density;
                if (mu > 0 && sigma > 0)
                {
                    double u1 = 1.0 - rnd;
                    double u2 = 1.0 - rng.NextDouble();

                    // Box–Muller transform
                    double standardNormal =
                        Math.Sqrt(-2.0 * Math.Log(u1)) *
                        Math.Cos(2.0 * Math.PI * u2);

                    // Scale and shift
                    double value = mu + sigma * standardNormal;

                    // Clamp to [0,1] for pixel use
                    var v = Math.Min(1.0, Math.Max(0.0, value));

                    initBuffer[4 * i + 0] = (float)v;
                }



                initBuffer[4 * i + 1] = 0f;
                initBuffer[4 * i + 2] = 0f;
                initBuffer[4 * i + 3] = 0f;

                var x = i % size;
                var y = size - 1 - i / size;
                var cx = centerX * size;
                var cy = centerY * size;
                var distX = MathUtil.GetTorusDistance(x, cx, size);
                var distY = MathUtil.GetTorusDistance(y, cy, size);
                var r = Math.Sqrt(distX * distX + distY * distY);
                if (r < blobRadius * size)
                    initBuffer[4 * i + 0] = 1.0f;

                if (r > noiseRadius * size)
                    initBuffer[4 * i + 0] = 0.0f;
            }
        }
    }
}
